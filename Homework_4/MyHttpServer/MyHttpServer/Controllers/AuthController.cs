using MyHttpServer.Framework;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Framework.HttpResponse;

namespace MyHttpServer.Contollers;

[HttpController("auth")]
internal class AuthController /*: ControllerBase*/
{
    [Get("login")]
    public string Login()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "static", "Steam", "login.html");
        if (!File.Exists(path))
        {
            path = Path.Combine(AppContext.BaseDirectory, "static", "Steam", "login.html");
        }
        return File.ReadAllText(path);
    }

    [Post("login")]
    public string Login(string login, string password)
    {
        Console.WriteLine($"Логин: {login}, пароль: {password}");
        
        string path = Path.Combine(Directory.GetCurrentDirectory(), "static", "Steam", "login.html");
        if (!File.Exists(path))
            path = Path.Combine(AppContext.BaseDirectory, "static", "Steam", "login.html");
        return File.ReadAllText(path);
    }

    /*[Get("html")]
    public byte[] Html()
    {
        string html = $"""
                       <!doctype html>
                       <html lang="ru"><meta charset="utf-8"><title>Результат формы</title>
                       <body>
                       </body></html>
                       """;
        return Html(html);
    }

    [Get("json")]
    public JsonResult Json()
    {
        var json = new { Username = "user", Password  = "12345", Age=13 };
        return Json(json);
    }

    [Get("json2")]
    public JsonResult Json2
    {
        var json = new { Username}
    }*/
    
    
}