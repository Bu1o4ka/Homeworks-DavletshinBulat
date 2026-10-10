namespace MyHttpServer.Framework.Attributes;
[AttributeUsage(AttributeTargets.Class)]
public class HttpControllerAttribute : Attribute
{
    
    public string Route { get; init; }

    public HttpControllerAttribute(string route)
    {
        Route = route;
    }
}