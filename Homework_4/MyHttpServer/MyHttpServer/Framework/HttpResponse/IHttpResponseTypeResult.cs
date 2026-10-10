using System.Net;

namespace MyHttpServer.Framework.HttpResponse;

public interface IHttpResponseTypeResult
{
    byte[] Execute(HttpListenerContext context);
}