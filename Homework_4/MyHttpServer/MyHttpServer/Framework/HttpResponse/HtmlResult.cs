using System.Net;
using System.Text;
using System.Text.Json;

namespace MyHttpServer.Framework.HttpResponse;

internal class HtmlResult
{
    private readonly object _html;

    public HtmlResult(object html)
    {
        _html = html;
    }
    public byte[] Execute(HttpListenerContext context)
    {
        var json = JsonSerializer.Serialize(_html);
        
        context.Response.ContentType = "text/html";
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        return buffer;
    }
}