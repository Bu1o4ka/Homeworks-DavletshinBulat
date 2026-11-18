using Microsoft.VisualStudio.TestTools.UnitTesting;
using MyHttpServer.Contollers;
using MyHttpServer.Controllers;

namespace MyHttpServer.Tests;

[TestClass]
public class ControllerTests
{
    [TestMethod]
    public void AuthController_Get()
    {
        var controller = new AuthController();
        var result = controller.Login();
        Assert.IsTrue(result.Length > 0);
    }

    [DataTestMethod]
    [DataRow("admin", "123")]
    [DataRow("", "")]
    [DataRow("user@mail.ru", "password")]
    public void AuthController_Post(string login, string pass)
    {
        var controller = new AuthController();
        var result = controller.Login(login, pass);
        Assert.IsTrue(result.Length > 0);
    }

    [DataTestMethod]
    [DataRow("test@test.ru")]
    [DataRow("")]
    [DataRow("invalid-email")]
    public async Task HomeworkController_MissingFile(string to)
    {
        var controller = new HomeworkController();
        var result = await controller.Send(to);
        Assert.IsTrue(result.Contains("Файл не найден") || result.Contains("Ошибка"));
    }
}