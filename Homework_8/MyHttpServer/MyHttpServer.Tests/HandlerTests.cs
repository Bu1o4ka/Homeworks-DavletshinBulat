using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Handlers;
using System.Net;

namespace MyHttpServer.Tests;

[TestClass]
public class HandlerTests
{
    private class MockHandler : Handler
    {
        public bool Called { get; private set; }
        public override Task HandleRequest(HttpListenerContext context) { Called = true; return Task.CompletedTask; }
    }

    [DataTestMethod]
    [DataRow("index.html")]
    [DataRow("style.css")]
    [DataRow("script.js")]
    [DataRow("image.png")]
    [DataRow("favicon.ico")]
    [DataRow("image.jpg")]
    [DataRow("icon.svg")]
    [DataRow("missing.txt")]
    public async Task StaticFileHandler_MissingFiles(string path)
    {
        var context = await TestHelpers.CreateContext(path);
        var handler = new StaticFileHandler();
        await handler.HandleRequest(context);
        Assert.IsTrue(context.Response.StatusCode == 404);
    }

    [TestMethod]
    public async Task StaticFileHandler_NotAFile_PassesNext()
    {
        var mock = new MockHandler();
        var handler = new StaticFileHandler { Successor = mock };
        var context = await TestHelpers.CreateContext("api/users");
        
        await handler.HandleRequest(context);
        Assert.IsTrue(mock.Called);
    }

    [DataTestMethod]
    [DataRow("nonexistent/method")]
    [DataRow("home-work/unknown")]
    public async Task ControllerHandler_NotFound(string path)
    {
        var handler = new ControllerHandler();
        var context = await TestHelpers.CreateContext(path);
        await handler.HandleRequest(context);
        Assert.IsTrue(context.Response.StatusCode == 404);
    }

    [TestMethod]
    public async Task ControllerHandler_IsFile_PassesNext()
    {
        var mock = new MockHandler();
        var handler = new ControllerHandler { Successor = mock };
        var context = await TestHelpers.CreateContext("image.png");
        
        await handler.HandleRequest(context);
        Assert.IsTrue(mock.Called);
    }
}