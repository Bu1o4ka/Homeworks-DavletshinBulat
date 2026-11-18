using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Net;

namespace MyHttpServer.Tests;

[TestClass]
public class ServerIntegrationTests
{
    private static HttpServer _server = null!;
    private static HttpClient _client = null!;
    private const string Port = "9997";

    [ClassInitialize]
    public static void Setup(TestContext context)
    {
        File.WriteAllText("settings.json", $@"{{ ""Server"": {{ ""Port"": ""{Port}"", ""Host"": ""localhost"", ""Path"": """" }} }}");
        _server = new HttpServer();
        _ = Task.Run(() => _server.Start());
        Task.Delay(1000).Wait();
        _client = new HttpClient { BaseAddress = new Uri($"http://localhost:{Port}/") };
    }

    [ClassCleanup]
    public static void Cleanup() 
    { 
        _server.Stop(); 
    }

    [DataTestMethod]
    [DataRow("unknown/route")]
    [DataRow("api/v1/users")]
    [DataRow("home-work/unknown")]
    public async Task Server_404(string route)
    {
        var res = await _client.GetAsync(route);
        Assert.IsTrue(res.StatusCode == HttpStatusCode.NotFound);
    }

    [TestMethod]
    public async Task Server_Get_Auth()
    {
        var res = await _client.GetAsync("auth/login");
        Assert.IsTrue(res.StatusCode == HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task Server_Post_Auth()
    {
        var data = new Dictionary<string, string> { { "login", "user" }, { "password", "123" } };
        var res = await _client.PostAsync("auth/login", new FormUrlEncodedContent(data));
        Assert.IsTrue(res.StatusCode == HttpStatusCode.OK);
    }

    [TestMethod]
    public async Task Server_Get_Homework()
    {
        var res = await _client.GetAsync("home-work/send?to=test@test.ru");
        Assert.IsTrue(res.StatusCode == HttpStatusCode.OK);
    }
}