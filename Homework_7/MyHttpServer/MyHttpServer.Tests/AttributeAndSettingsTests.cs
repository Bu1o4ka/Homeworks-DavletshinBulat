using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Framework.Attributes;
using System.Text.Json;

namespace MyHttpServer.Tests;

[TestClass]
public class AttributeAndSettingsTests
{
    [DataTestMethod]
    [DataRow("test1")]
    [DataRow("")]
    [DataRow("api/v1")]
    public void Attributes(string route)
    {
        Assert.IsTrue(new GetAttribute(route).Route == route);
        Assert.IsTrue(new PostAttribute(route).Route == route);
        Assert.IsTrue(new HttpControllerAttribute(route).Route == route);
    }

    [TestMethod]
    public void Settings_Deserialize()
    {
        string json = @"{ ""Server"": { ""Port"": ""1234"", ""Host"": ""127.0.0.1"", ""Path"": ""test"" } }";
        var s = JsonSerializer.Deserialize<Settings>(json);
        Assert.IsTrue(s != null);
        Assert.IsTrue(s!.Server.Port == "1234");
        Assert.IsTrue(s.Server.Host == "127.0.0.1");
    }

    [TestMethod]
    public void Settings_Defaults()
    {
        var s = new Server();
        Assert.IsTrue(s.Port == "8888");
        Assert.IsTrue(s.Host == "localhost");
        Assert.IsTrue(s.Path == "");
    }
}