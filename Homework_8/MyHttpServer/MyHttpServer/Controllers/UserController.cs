using System.Text.Json;
using MyHttpServer.Framework.Attributes;
using MyHttpServer.Models;
using MyHttpServer.ORM;

namespace MyHttpServer.Controllers;

[HttpController("user")]
public class UserController
{
    [Get("list")]
    public async Task<string> List()
    {
        var json = File.ReadAllText("settings.json");
        var settings = JsonSerializer.Deserialize<Settings>(json)!;

        IOrmContext orm = new OrmContext(settings.ConnectionString);
        IReadOnlyList<User> users = await orm.GetAllAsync<User>("Users");

        var renderer = new MiniTemplateEngine.HtmlTemplateRenderer();
        
        string path = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "User", "List.html");
        if (!File.Exists(path))
            path = Path.Combine(AppContext.BaseDirectory, "Templates", "User", "List.html");

        return renderer.RenderFromFile(path, new { Users = users });
    }
}
