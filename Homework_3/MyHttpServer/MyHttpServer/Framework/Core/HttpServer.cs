using System.Net;
using System.Text;
using System.Text.Json;

namespace MyHttpServer;

public class HttpServer
{
    private readonly HttpListener server = new HttpListener();
    public async Task Start()
    {
        
        string settingsJson = File.ReadAllText("settings.json");
        Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);
        string uniPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}/{setting.Server.Path}/";
        server.Prefixes.Add(uniPrefix);
        server.Start();
        Console.WriteLine("Сервер работает и слушает: " + uniPrefix);
        while (true)
        {
            var context = await server.GetContextAsync();
            var request = context.Request;
            Console.WriteLine("Пришел запрос");
            
            string path = request.Url.LocalPath.TrimStart('/');
            string filePath = Path.Combine(Directory.GetCurrentDirectory(), path);
            HttpListenerResponse response = context.Response;
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
            Console.WriteLine("Запрос обработан");
        }
    }

    public void Stop()
    {
        server.Stop();
    }
}