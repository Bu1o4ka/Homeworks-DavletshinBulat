using System.Net;
using System.Text;
using System.Text.Json;

namespace ControlWork;

public class HttpServer
{
    private readonly HttpListener server = new HttpListener();
    private BookStore store = new BookStore();
    private string basePath = "/";

    public async Task Start()
    {
        string settingsJson = File.ReadAllText("settings.json");
        Settings setting = JsonSerializer.Deserialize<Settings>(settingsJson);
        string pathPart = setting.Server.Path.Trim('/');
        string uniPrefix = $"http://{setting.Server.Host}:{setting.Server.Port}/";
        if (!string.IsNullOrEmpty(pathPart))
        {
            uniPrefix += $"{pathPart}/";
            basePath = "/" + pathPart;
        }
        else
        {
            basePath = "";
        }

        server.Prefixes.Add(uniPrefix);
        server.Start();
        Console.WriteLine("Сервер работает и слушает: " + uniPrefix);
        while (true)
        {
            var context = await server.GetContextAsync();
            var request = context.Request;
            var response = context.Response;
            Console.WriteLine("Пришел запрос: " + request.HttpMethod + " " + request.Url.AbsolutePath);
            await Handle(request, response);

            Console.WriteLine("Запрос обработан");
        }
    }

    private async Task Handle(HttpListenerRequest request, HttpListenerResponse response)
    {
        string path = request.Url.AbsolutePath;
        
        if (!string.IsNullOrEmpty(basePath) && path.StartsWith(basePath))
            path = path.Substring(basePath.Length);
            
        if (path == "") path = "/";
        if (path.Length > 1 && path.EndsWith("/"))
            path = path.Substring(0, path.Length - 1);

        string method = request.HttpMethod;

        if (method == "GET" && path == "/books")
            await ShowBooks(request, response);
        else if (method == "GET" && path == "/books/details")
            await ShowDetails(request, response);
        else if (method == "POST" && path == "/books/create")
            await CreateBook(request, response);
        else if (method == "PUT" && path == "/books/update")
            await UpdateBook(request, response);
        else
            await Send(response, 404, "text/html; charset=utf-8", "<p>404</p>");
    }

    private async Task ShowBooks(HttpListenerRequest request, HttpListenerResponse response)
    {
        string author = request.QueryString["author"];
        if (author == null) 
        {
            author = "";
        }
        List<Book> books = store.GetAll(author);

        string html = "<h1>Книги</h1>";
        html += "<form method='get' action='" + basePath + "/books'>";
        html += "<input name='author' value='" + WebUtility.HtmlEncode(author) + "'>";
        html += "<button>Найти</button>";
        html += "</form>";

        if (books.Count == 0)
        {
            html += "<p>Пусто</p>";
        }
        else
        {
            html += "<ul>";
            foreach (var b in books)
            {
                html += "<li><a href='" + basePath + "/books/details?id=" + b.Id + "'>" + WebUtility.HtmlEncode(b.Title) + "</a>" + " =) ";
                html += WebUtility.HtmlEncode(b.Author) + " (" + b.Status + ")</li>";
            }
            html += "</ul>";
        }

        html += "<h1>Добавить</h1>";
        html += "<form method='post' action='" + basePath + "/books/create'>";
        html += "Название: <input name='title'><br>";
        html += "Автор: <input name='author'><br>";
        html += "<button>Создать</button>";
        html += "</form>";

        await Send(response, 200, "text/html; charset=utf-8", html);
    }

    private async Task ShowDetails(HttpListenerRequest request, HttpListenerResponse response)
    {
        int id;
        int.TryParse(request.QueryString["id"], out id);

        Book b = store.Get(id);
        if (b == null)
        {
            await Send(response, 404, "text/html; charset=utf-8", "<p>404</p>");
            return;
        }

        string html = "<h1>" + WebUtility.HtmlEncode(b.Title) + "</h1>";
        html += "<p>Автор: " + WebUtility.HtmlEncode(b.Author) + "</p>";
        html += "<p>Статус: " + b.Status + "</p>";
        html += "<h3>Редактировать</h3>";
        html += "<form id='editForm'>";
        html += "Название: <input name='title' value='" + WebUtility.HtmlEncode(b.Title) + "'><br>";
        html += "Автор: <input name='author' value='" + WebUtility.HtmlEncode(b.Author) + "'><br>";
        html += "Статус: <input name='status' value='" + WebUtility.HtmlEncode(b.Status) + "'><br><br>";
        html += "<button type='button' onclick='saveBook()'>Сохранить</button>";
        html += "</form>";
        html += "<p><a href='" + basePath + "/books'>Назад</a></p>";

        
        html += "<script>";
        html += "function saveBook() {";
        html += "  var form = document.getElementById('editForm');";
        html += "  var data = new URLSearchParams(new FormData(form));";
        html += "  fetch('" + basePath + "/books/update?id=" + b.Id + "', {";
        html += "    method: 'PUT',";
        html += "    body: data";
        html += "  }).then(function(res) {";
        html += "    window.location.href = '" + basePath + "/books';";
        html += "  });";
        html += "}";
        html += "</script>";
        await Send(response, 200, "text/html; charset=utf-8", html);
    }

    private async Task CreateBook(HttpListenerRequest request, HttpListenerResponse response)
    {
        var form = await ReadForm(request);
        string title = form.ContainsKey("title") ? form["title"] : "";
        string author = form.ContainsKey("author") ? form["author"] : "";

        store.Create(title, author);
        
        response.StatusCode = 303;
        response.Headers["Location"] = basePath + "/books";
        response.ContentLength64 = 0;
        response.Close();
    }

    private async Task UpdateBook(HttpListenerRequest request, HttpListenerResponse response)
    {
        int id;
        int.TryParse(request.QueryString["id"], out id);

        var form = await ReadForm(request);
        string title = form.ContainsKey("title") ? form["title"] : "";
        string author = form.ContainsKey("author") ? form["author"] : "";
        string status = form.ContainsKey("status") ? form["status"] : "";

        string error = store.Update(id, title, author, status);
        if (error == "NOT_FOUND")
        {
            await Send(response, 404, "text/plain; charset=utf-8", "Книга не найдена");
            return;
        }
        
        response.ContentLength64 = 0;
        response.Close();
    }

    private async Task<Dictionary<string, string>> ReadForm(HttpListenerRequest request)
    {
        Dictionary<string, string> form = new Dictionary<string, string>();
        using (var reader = new StreamReader(request.InputStream, Encoding.UTF8))
        {
            string body = await reader.ReadToEndAsync();
            foreach (var pair in body.Split('&'))
            {
                int eq = pair.IndexOf('=');
                if (eq < 0) continue;
                string key = Uri.UnescapeDataString(pair.Substring(0, eq).Replace('+', ' '));
                string value = Uri.UnescapeDataString(pair.Substring(eq + 1).Replace('+', ' '));
                form[key] = value;
            }
        }
        return form;
    }

    private async Task Send(HttpListenerResponse response, int status, string contentType, string text)
    {
        response.StatusCode = status;
        response.ContentType = contentType;
        byte[] buffer = Encoding.UTF8.GetBytes(text);
        response.ContentLength64 = buffer.Length;
        using Stream output = response.OutputStream;
        await output.WriteAsync(buffer);
        await output.FlushAsync();
    }

    public void Stop()
    {
        server.Stop();
    }
}