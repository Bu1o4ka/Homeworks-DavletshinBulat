using System.Net;
using MyHttpServer.Framework.HttpResponse;

namespace MyHttpServer.Framework;

internal class ControllerBase
{
    protected HttpListenerContext Context { get; set; }
    
    internal void SeetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected JsonResult Json(object obj) => new JsonResult(obj);
    
    protected HtmlResult Html(string html) => new HtmlResult(html);
}