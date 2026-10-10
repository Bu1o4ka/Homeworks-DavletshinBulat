using System.Net;
using System.Reflection;
using System.Text;
using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Framework.Handlers;

internal class ControllerHandler: Handler
{
    public override async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        HttpListenerResponse response = context.Response;
        string path = request.Url.LocalPath.TrimStart('/');
        bool isFile = Path.HasExtension(path);
        if (!isFile)
        {
            string[] strParams = context.Request.Url 
                .Segments 
                .Skip(2) 
                .Select(s => s.Replace("/", "")) 
                .ToArray(); 
 
            var assembly = Assembly.GetExecutingAssembly(); 
            var segments = context.Request.Url.Segments;
            if (segments.Length < 2)
            {
                response.StatusCode = 404;
                await WriteResponseAsync(response, "Контроллер не указан");
                return;
            }
            string controllerName = segments[1].Trim('/');
            string methodName = segments.Length > 2 ? segments[2].Trim('/') : "";
            var controller = assembly.GetTypes()
                .Where(t => Attribute.IsDefined(t, typeof(HttpControllerAttribute)))
                .FirstOrDefault(c => c.Name.ToLower() == controllerName.ToLower() + "controller");
 
            if (controller == null)
            {
                response.StatusCode = 404;
                await WriteResponseAsync(response, "Контроллер не найден");
                return;
            } 

            var method = controller.GetMethods().FirstOrDefault(m =>
            {
                if (request.HttpMethod.Equals("GET", StringComparison.OrdinalIgnoreCase))
                {
                    var attr = m.GetCustomAttribute<GetAttribute>();
                    return attr != null && attr.Route.Equals(methodName, StringComparison.OrdinalIgnoreCase);
                }
                if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
                {
                    var attr = m.GetCustomAttribute<PostAttribute>();
                    return attr != null && attr.Route.Equals(methodName, StringComparison.OrdinalIgnoreCase);
                }
                return false;
            }); 
 
            if (method == null)
            {
                response.StatusCode = 404;
                await WriteResponseAsync(response, "Метод не найден");
                return;
            } 
 
            object[] queryParams;

            if (request.HttpMethod.Equals("POST", StringComparison.OrdinalIgnoreCase))
            {
                string body;
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding ?? Encoding.UTF8))
                    body = await reader.ReadToEndAsync();

                var form = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                foreach (var pair in body.Split('&', StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = pair.Split('=', 2);
                    form[Uri.UnescapeDataString(kv[0].Replace('+', ' '))] =
                        kv.Length > 1 ? Uri.UnescapeDataString(kv[1].Replace('+', ' ')) : "";
                }

                queryParams = method.GetParameters()
                    .Select(p => (object)(p.Name != null && form.TryGetValue(p.Name, out var v) ? v : ""))
                    .ToArray();
            }
            else
            {
                queryParams = method.GetParameters()
                    .Select((p, i) => i < strParams.Length
                        ? Convert.ChangeType(strParams[i], p.ParameterType)
                        : (object)"")
                    .ToArray();
            }

            var ret = method.Invoke(Activator.CreateInstance(controller), queryParams);

            if (ret is Task task) { await task; ret = null; }

            if (ret is string htmlResult)
            {
                response.ContentType = "text/html; charset=utf-8";
                await WriteResponseAsync(response, htmlResult);
            }
            else
            {
                response.StatusCode = 200;
                response.ContentType = "text/html; charset=utf-8";
                await WriteResponseAsync(response, "OK");
            }
        }
        else if (Successor != null)
        {
            Successor.HandleRequest(context);
        }
        else
        {
            response.StatusCode = 404;
            await WriteResponseAsync(response, "404 Not Found");
        }
        
    }

    private static async Task WriteResponseAsync(HttpListenerResponse response, string content)
    {
        response.ContentType = "text/html; charset=utf-8";
        byte[] buffer = Encoding.UTF8.GetBytes(content);
        response.ContentLength64 = buffer.Length;
        await response.OutputStream.WriteAsync(buffer);
        response.OutputStream.Close();
    }
}