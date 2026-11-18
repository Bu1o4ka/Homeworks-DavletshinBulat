using System.Net;
using System.Text;

namespace MyHttpServer.Tests;

public static class TestHelpers
{
    public static async Task<HttpListenerContext> CreateContext(string urlPath = "", string method = "GET", string body = "")
    {
        int port = new Random().Next(10000, 60000);
        var listener = new HttpListener();
        listener.Prefixes.Add($"http://localhost:{port}/");
        listener.Start();

        var contextTask = listener.GetContextAsync();

        using var client = new HttpClient();
        var url = $"http://localhost:{port}/{urlPath.TrimStart('/')}";
        
        try
        {
            if (method == "POST")
            {
                var content = new StringContent(body, Encoding.UTF8, "application/x-www-form-urlencoded");
                _ = client.PostAsync(url, content);
            }
            else
            {
                _ = client.GetAsync(url);
            }

            var context = await contextTask;
            return context;
        }
        catch
        {
            listener.Stop();
            throw;
        }
    }
}