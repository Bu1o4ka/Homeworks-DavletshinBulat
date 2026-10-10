using System.Net;
using System.Text;
using System.Text.Json;
using MyHttpServer.Framework.Handlers;


namespace MyHttpServer;

public class HttpServer
{
    private readonly HttpListener server = new HttpListener();
    public async Task Start()
    {
        
        string settingsJson = File.ReadAllText("settings.json");
        Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);
        string uniPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}/";
        if (!string.IsNullOrEmpty(setting.Server.Path))
        {
            uniPrefix += $"{setting.Server.Path}/";
        }
        server.Prefixes.Add(uniPrefix);
        server.Start();
        Console.WriteLine("Сервер работает и слушает: " + uniPrefix);
        while (true)
        {
            var context = await server.GetContextAsync();
            await ProcessRequestAsync(context);
        }
    }
    public void Stop()
    {
        server.Stop();
    }

    private async Task ProcessRequestAsync(HttpListenerContext context)
    {
        Handler staticFileHandler = new StaticFileHandler();
        Handler controllerHandler = new ControllerHandler();
        staticFileHandler.Successor = controllerHandler;
        Console.WriteLine("Пришел запрос");
        await staticFileHandler.HandleRequest(context);
        Console.WriteLine($"Обработан запрос: {context.Request.Url}");
    }
}