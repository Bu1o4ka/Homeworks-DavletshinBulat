using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.HttpResponse;
using System.Text;

namespace MyHttpServer.Tests;

[TestClass]
public class ResponseTests
{
    [DataTestMethod]
    [DataRow("<h1>Hello</h1>")]
    [DataRow("")]
    [DataRow("Просто текст")]
    public async Task HtmlResult_ReturnsString(string html)
    {
        var context = await TestHelpers.CreateContext();
        var result = new HtmlResult(html);
        byte[] bytes = result.Execute(context);
        
        string expected = System.Text.Json.JsonSerializer.Serialize(html);
        Assert.IsTrue(context.Response.ContentType == "text/html");
        Assert.IsTrue(Encoding.UTF8.GetString(bytes) == expected);
    }

    [TestMethod]
    public async Task JsonResult_ReturnsJson()
    {
        var context = await TestHelpers.CreateContext();
        var result = new JsonResult(new { Name = "Ivan" });
        byte[] bytes = result.Execute(context);
        
        Assert.IsTrue(Encoding.UTF8.GetString(bytes) == "{\"Name\":\"Ivan\"}");
    }
}