using System.Net;
using System.Text;

namespace MyHttpServer.Framework.Handlers;

class StaticFileHandler: Handler
{
    public override async Task HandleRequest(HttpListenerContext context)
    {
        var request = context.Request;
        HttpListenerResponse response = context.Response;
        string path = request.Url.LocalPath.TrimStart('/');
        bool isFile = path.Contains(".");
        if (isFile)
        {
            try
            {
                
                string filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
                // if (!File.Exists(filePath))
                // {
                //     string fallbackPath = Path.Combine(Directory.GetCurrentDirectory(), "static", "Login_Form", path);
                //     if (File.Exists(fallbackPath))
                //     {
                //         filePath = fallbackPath;
                //     }
                // }

                    
                FileInfo fileInfo = new FileInfo(filePath);

                if (!fileInfo.Exists)
                {
                    response.StatusCode = 404;
                    filePath = Directory.GetCurrentDirectory() + "/static/404.html";
                    fileInfo = new FileInfo(filePath);
                }

                switch (fileInfo.Extension)
                {
                    case ".html":
                        response.ContentType = "text/html; charset=utf-8";
                        break;
                    case ".css":
                        response.ContentType = "text/css; charset=utf-8";
                        break;
                    case ".js":
                        response.ContentType = "text/javascript; charset=utf-8";
                        break;
                    case ".png":
                        response.ContentType = "image/png";
                        break;
                    case ".ico":
                        response.ContentType = "image/x-icon";
                        break;
                    case ".svg":
                        response.ContentType = "image/svg+xml";
                        break;
                    case ".jpg":
                        response.ContentType = "image/jpeg";
                        break;
                }

                byte[] buffer = await File.ReadAllBytesAsync(filePath);
                response.ContentLength64 = buffer.Length;
                using Stream output = response.OutputStream;
                await output.WriteAsync(buffer);
                await output.FlushAsync();
            
                
            }
            catch (Exception ex)
            {
                response.StatusCode = 500;
                await WriteResponseAsync(response, $"Ошибка при обработке запроса: {ex.Message}");
            }
        }
        else if (Successor != null)
        {
            await Successor.HandleRequest(context);
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