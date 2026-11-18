using System.Net;
using MyHttpServer.Framework.HttpResponse;

namespace MyHttpServer.Framework;

public class ControllerBase
{
    protected HttpListenerContext Context { get; set; }
    
    public void SetContext(HttpListenerContext context)
    {
        Context = context;
    }

    protected JsonResult Json(object obj) => new JsonResult(obj);
    
    protected HtmlResult Html(string html) => new HtmlResult(html);
}