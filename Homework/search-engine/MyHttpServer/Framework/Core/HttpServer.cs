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
            
            
            HttpListenerResponse response = context.Response;
            
            byte[] buffer = Encoding.UTF8.GetBytes(File.ReadAllText("Search.html"));
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