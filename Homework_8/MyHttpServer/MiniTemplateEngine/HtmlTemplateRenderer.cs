using System.Collections;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Reflection;
using System.Text;

namespace MiniTemplateEngine;

/// <summary>
/// Реализация простого HTML-шаблонизатора с поддержкой переменных, условий и циклов.
/// </summary>
public class HtmlTemplateRenderer : IHtmlTemplateRenderer
{
    // Регулярное выражение для поиска переменных вида ${Name} или ${obj.Prop}
    private static readonly Regex VarRegex = new Regex(@"\$\{([^}]+)\}", RegexOptions.Compiled);

    // Регулярное выражение для поиска циклов $foreach ... $endfor
    private static readonly Regex ForEachRegex = new Regex(
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

    // Регулярное выражение для поиска условий $if ... [$else ...] $endif
    private static readonly Regex IfRegex = new Regex(
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

    /// <summary>
    /// Рендерит HTML-шаблон из строки.
    /// </summary>
    public string RenderFromString(string htmlTemplate, object dataModel)
    {
        // Создаем пустой словарь для хранения локальных переменных (например, переменных циклов)
        var scope = new Dictionary<string, object>(StringComparer.Ordinal);
        // Вызываем основной метод рендеринга, передавая пустой scope
        return Render(htmlTemplate ?? "", dataModel, scope);
    }

    /// <summary>
    /// Рендерит HTML-шаблон из файла.
    /// </summary>
    public string RenderFromFile(string filePath, object dataModel)
    {
        // Читаем весь текст из файла
        string template = File.ReadAllText(filePath);
        // Передаем прочитанный текст в метод RenderFromString
        return RenderFromString(template, dataModel);
    }

    /// <summary>
    /// Рендерит из файла и сохраняет результат в другой файл.
    /// </summary>
    public string RenderToFile(string inputFilePath, string outputFilePath, object dataModel)
    {
        // Получаем готовую строку через рендеринг файла
        var result = RenderFromFile(inputFilePath, dataModel);
        // Записываем полученную строку в выходной файл
        File.WriteAllText(outputFilePath, result);
        // Возвращаем результат
        return result;
    }

    // Внутренний движок рендеринга
    private string Render(string template, object model, Dictionary<string, object> scope)
    {
        // Если шаблон пустой, возвращаем пустую строку
        if (string.IsNullOrEmpty(template)) return "";

        // Переменная для отслеживания изменений в строке (для вложенных конструкций)
        string prev;
        
        // --- ЭТАП 1: Обработка циклов (foreach) ---
        do
        {
            // Сохраняем текущее состояние шаблона
            prev = template;
            // Ищем совпадения с регуляркой циклов и заменяем их
            template = ForEachRegex.Replace(template, m =>
            {
                // Извлекаем имя переменной-итератора (например, p)
                string item = m.Groups["item"].Value;
                // Извлекаем путь к коллекции (например, shop.Pens)
                string coll = m.Groups["coll"].Value;
                // Пытаемся получить коллекцию с помощью рефлексии
                var collection = Resolve(coll, model, scope);
                
                // Если это не коллекция или это строка, прерываем (защита от ошибок)
                if (collection is not IEnumerable seq || collection is string) return "";
                
                // Создаем StringBuilder для накопления результата итераций
                var sb = new StringBuilder();
                // Проходимся по каждому элементу коллекции
                foreach (var it in seq)
                {
                    // Создаем новый контекст, копируя текущий, и добавляем переменную итератора
                    var inner = new Dictionary<string, object>(scope, StringComparer.Ordinal) { [item] = it };
                    // Рекурсивно вызываем Render для тела цикла с новым контекстом
                    sb.Append(Render(m.Groups["body"].Value, model, inner));
                }
                // Возвращаем склеенный результат
                return sb.ToString();
            });
        } while (template != prev); // Повторяем, пока находятся внешние циклы

        // --- ЭТАП 2: Обработка условий (if/else) ---
        do
        {
            // Сохраняем текущее состояние
            prev = template;
            // Ищем условия и заменяем их
            template = IfRegex.Replace(template, m =>
            {
                // Вычисляем условие (true/false)
                bool ok = EvalCondition(m.Groups["cond"].Value, model, scope);
                // Если условие истинно, рендерим блок "then"
                if (ok) return Render(m.Groups["then"].Value, model, scope);
                // Если есть блок "else" и условие ложно, рендерим его
                if (m.Groups["else"].Success) return Render(m.Groups["else"].Value, model, scope);
                // Если блока else нет, возвращаем пустоту
                return "";
            });
        } while (template != prev); // Повторяем, пока находятся условия

        // --- ЭТАП 3: Подстановка переменных ${...} ---
        // Заменяем оставшиеся переменные их значениями, полученными через рефлексию
        return VarRegex.Replace(template, m => Resolve(m.Groups[1].Value, model, scope)?.ToString() ?? "");
    }

    // Метод для получения значения свойства по его пути (например, user.Name)
    private static object? Resolve(string path, object? model, Dictionary<string, object> scope)
    {
        // Проверка на пустой путь
        if (string.IsNullOrWhiteSpace(path)) return null;
        // Разбиваем путь по точкам (например, ["user", "Name"])
        var parts = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
        // Если частей нет, возвращаем null
        if (parts.Length == 0) return null;

        object? cur;
        int i;

        // Проверяем, есть ли первая часть пути в нашем локальном контексте (в цикле)
        if (scope.TryGetValue(parts[0], out var scoped))
        {
            cur = scoped; i = 1; // Начинаем со следующей части пути
        }
        else
        {
            // Ищем свойство с таким именем в основной модели данных (игнорируя регистр)
            var firstOnModel = model?.GetType().GetProperty(parts[0], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            // Если нашли, берем значение
            if (firstOnModel != null) { cur = firstOnModel.GetValue(model); i = 1; }
            // Если не нашли, возможно, корень - это сама модель (например, ${Name})
            else { cur = model; i = 0; } 
        }

        // Проходим по остальным частям пути (например, "Name" от "user")
        for (; i < parts.Length; i++)
        {
            if (cur == null) return null; // Если промежуточное значение null, дальше идти нельзя
            // Ищем свойство в текущем объекте
            var p = cur.GetType().GetProperty(parts[i], BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (p == null) return null; // Свойство не найдено
            cur = p.GetValue(cur); // Получаем значение свойства
        }
        return cur; // Возвращаем итоговое значение
    }

    // Метод для вычисления логического условия
    private static bool EvalCondition(string raw, object model, Dictionary<string, object> scope)
    {
        // Сначала подставляем значения всех переменных в строку условия
        string s = VarRegex.Replace(raw, m => Resolve(m.Groups[1].Value, model, scope)?.ToString() ?? "").Trim();

        // Ищем операторы сравнения (==, !=, >=, <=, >, <)
        var cmp = Regex.Match(s, @"^(.*?)\s*(==|!=|>=|<=|>|<)\s*(.*)$");
        if (cmp.Success) // Если оператор найден
        {
            // Извлекаем левую часть, оператор и правую часть, убирая лишние кавычки
            string l = cmp.Groups[1].Value.Trim().Trim('"', '\'');
            string op = cmp.Groups[2].Value;
            string r = cmp.Groups[3].Value.Trim().Trim('"', '\'');

            // Пытаемся распарсить обе части как числа (для числового сравнения)
            if (double.TryParse(l, NumberStyles.Any, CultureInfo.InvariantCulture, out var ln) &&
                double.TryParse(r, NumberStyles.Any, CultureInfo.InvariantCulture, out var rn))
            {
                // Выполняем математическое сравнение в зависимости от оператора
                return op switch
                {
                    "==" => ln == rn, "!=" => ln != rn,
                    ">=" => ln >= rn, "<=" => ln <= rn,
                    ">"  => ln >  rn, "<"  => ln <  rn, _ => false
                };
            }

            // Если это не числа, сравниваем как строки (игнорируя регистр)
            int c = string.Compare(l, r, StringComparison.OrdinalIgnoreCase);
            return op switch
            {
                "==" => c == 0, "!=" => c != 0,
                ">=" => c >= 0, "<=" => c <= 0,
                ">"  => c >  0, "<"  => c <  0, _ => false
            };
        }

        // Если оператора нет, проверяем на boolean (true/false)
        if (bool.TryParse(s, out var b)) return b;
        // Или на число (0 = false, остальное = true)
        if (double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var d)) return d != 0;
        // Иначе проверяем, что строка не пустая
        return !string.IsNullOrEmpty(s);
    }
}