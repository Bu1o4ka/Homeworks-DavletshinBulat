using System.Net;
using System.Text;
using System.Text.Json;

namespace MyHttpServer.Framework.HttpResponse;

internal class JsonResult
{
    private readonly object _data;

    public JsonResult(object data)
    {
        _data = data;
    }

    public byte[] Execute(HttpListenerContext context)
    {
        var json = JsonSerializer.Serialize(_data);
        
        
        byte[] buffer = Encoding.UTF8.GetBytes(json);
        return buffer;
    }
    
}