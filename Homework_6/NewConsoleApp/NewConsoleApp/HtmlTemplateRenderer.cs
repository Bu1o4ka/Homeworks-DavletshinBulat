
using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

public class HtmlTemplateRenderer: IHtmlTemplateRenderer
{
    // ${Name} / ${user.Name} / ${p.Title}
    private static readonly Regex VarRegex = new(
        @"\$\{([^}]+)\}", RegexOptions.Compiled);

    // Самый ВНЕШНИЙ $foreach … $endfor (балансирующие группы считают вложенность)
    private static readonly Regex ForEachRegex = new(
        @"\$foreach\s+\$\{(?<item>\w+)\}\s+in\s+\$\{(?<coll>[^}]+)\}" +
        @"(?<body>" +
        @"(?:" +
        @"\$foreach\b(?<D>)" +
        @"|\$endfor\b(?<-D>)" +
        @"|(?!\$foreach\b|\$endfor\b)[\s\S]" +
        @")*" +
        @")" +
        @"(?(D)(?!))" +
        @"\$endfor\b",
        RegexOptions.Compiled);

    // Самый ВНУТРЕННИЙ $if … [$else …] … $endif
    private static readonly Regex IfRegex = new(
        @"\$if\s+" +
        @"(?<cond>\$\{[^}]+\}" +
        @"(?:[ \t]*(?:==|!=|>=|<=|>|<)[ \t]*" +
        @"(?:\$\{[^}]+\}|""[^""]*""|'[^']*'|-?\d+(?:\.\d+)?|true|false|\w+))?" +
        @")" +
        @"\s*" +
        @"(?<then>(?:(?!\$if\b|\$else\b|\$endif\b)[\s\S])*?)" +
        @"(?:\$else\s*(?<else>(?:(?!\$if\b|\$endif\b)[\s\S])*?))?" +
        @"\$endif\b",
        RegexOptions.Compiled);

    // ---------- Публичный API ----------

    public string RenderFromString(string htmlTemplate, object dataModel)
        => Render(htmlTemplate ?? "", dataModel,
                  new Dictionary<string, object>(StringComparer.Ordinal));

    public string RenderFromFile(string filePath, object dataModel)
        => RenderFromString(File.ReadAllText(filePath), dataModel);

    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        var s = RenderFromFile(inputFilePath, dataModel);
        File.WriteAllText(outputFilePath, s);
        return s;
    }

    // ---------- Движок: три прохода ----------

    private string Render(string template, object model, Dictionary<string, object> scope)
    {
        if (string.IsNullOrEmpty(template)) return "";

        // 1) foreach (самый внешний), тело рекурсивно рендерится
        string prev;
        do
        {
            prev = template;
            template = ForEachRegex.Replace(template, m =>
            {
                string item = m.Groups["item"].Value;
                string coll = m.Groups["coll"].Value;
                if (Resolve(coll, model, scope) is not IEnumerable seq || seq is string)
                    return "";
                var sb = new StringBuilder();
                foreach (var it in seq)
                {
                    var inner = new Dictionary<string, object>(scope, StringComparer.Ordinal)
                        { [item] = it };
                    sb.Append(Render(m.Groups["body"].Value, model, inner));
                }
                return sb.ToString();
            });
        } while (template != prev);

        // 2) if (самый вложенный — первым)
        do
        {
            prev = template;
            template = IfRegex.Replace(template, m =>
            {
                bool ok = EvalCondition(m.Groups["cond"].Value, model, scope);
                if (ok) return Render(m.Groups["then"].Value, model, scope);
                if (m.Groups["else"].Success) return Render(m.Groups["else"].Value, model, scope);
                return "";
            });
        } while (template != prev);

        // 3) добиваем оставшиеся ${...}
        return VarRegex.Replace(template, m =>
            Resolve(m.Groups[1].Value, model, scope)?.ToString() ?? "");
    }

    // ---------- Резолв пути ${...} ----------

    private static object? Resolve(string path, object? model, Dictionary<string, object> scope)
    {
        if (string.IsNullOrWhiteSpace(path)) return null;
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return null;

        object? cur;
        int i;

        if (scope.TryGetValue(parts[0], out var scoped))
        {
            cur = scoped; i = 1;                 // ${p.Title} — p из foreach
        }
        else
        {
            var firstOnModel = model?.GetType().GetProperty(parts[0],
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (firstOnModel != null) { cur = firstOnModel.GetValue(model); i = 1; }
            else { cur = model; i = 1; }         // ${user.Name} — user = корень
        }

        for (; i < parts.Length; i++)
        {
            if (cur == null) return null;
            var p = cur.GetType().GetProperty(parts[i],
                BindingFlags.Public | BindingFlags.NonPublic |
                BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null) return null;
            cur = p.GetValue(cur);
        }
        return cur;
    }

    // ---------- Логика условия ----------

    private static bool EvalCondition(string raw, object model, Dictionary<string, object> scope)
    {
        string s = VarRegex.Replace(raw, m =>
        {
            var v = Resolve(m.Groups[1].Value, model, scope);
            return v?.ToString() ?? "";
        }).Trim();

        var cmp = Regex.Match(s, @"^(.*?)\s*(==|!=|>=|<=|>|<)\s*(.*)$");
        if (cmp.Success)
        {
            string l = cmp.Groups[1].Value.Trim().Trim('"', '\'');
            string op = cmp.Groups[2].Value;
            string r = cmp.Groups[3].Value.Trim().Trim('"', '\'');

            if (double.TryParse(l, NumberStyles.Any, CultureInfo.InvariantCulture, out var ln) &&
                double.TryParse(r, NumberStyles.Any, CultureInfo.InvariantCulture, out var rn))
            {
                return op switch
                {
                    "==" => ln == rn, "!=" => ln != rn,
                    ">=" => ln >= rn, "<=" => ln <= rn,
                    ">"  => ln >  rn, "<"  => ln <  rn, _ => false
                };
            }

            int c = string.Compare(l, r, StringComparison.OrdinalIgnoreCase);
            return op switch
            {
                "==" => c == 0, "!=" => c != 0,
                ">=" => c >= 0, "<=" => c <= 0,
                ">"  => c >  0, "<"  => c <  0, _ => false
            };
        }

        if (bool.TryParse(s, out var b)) return b;
        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d != 0;
        return !string.IsNullOrEmpty(s);
    }
    
    // public string RenderFromString(string htmlTemplate, object dataModel)
    // {
    //     string pattern = "\\${(\\S*)}";
    //     Type myType = dataModel.GetType();
    //     Regex regex = new Regex(pattern);
    //     MatchCollection collection = regex.Matches(htmlTemplate);
    //     string result = regex.Replace(htmlTemplate, (match) =>
    //     {
    //         string fieldName = match.Groups[1].Value.Split('.')[1];
    //         var smth = myType.GetProperty(fieldName,  BindingFlags.IgnoreCase | BindingFlags.NonPublic | BindingFlags.Instance);
    //         Console.WriteLine(smth);
    //         return match.Groups[1].Value;
    //     });
    //     return htmlTemplate.Replace("${name}", dataModel.ToString());
    // }
    //
    // public string RenderFromFile(string filePath, object dataModel)
    // {
    //     
    //     throw new NotImplementedException();
    // }
    //
    // public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    // {
    //     throw new NotImplementedException();
    // }
    
}
