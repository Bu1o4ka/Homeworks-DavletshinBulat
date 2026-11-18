using NewConsoleApp;

string template = "<p>Имя: ${user.Name}</p>" +
                  "<p>Возраст: ${user.Age}</p>" +
                  "<p>Группа: ${user.Group}</p>" +
                  "<p>Пол: ${user.Sex}</p>";
User user = new User("Ваня", 22, "11-541", true);
string name = "Ваня";

HtmlTemplateRenderer htr = new HtmlTemplateRenderer();
string result = htr.RenderFromString(template, user);
Console.WriteLine(result);

string template1 = "<p>Название: ${book.Title}</p>" +
                   "<p>Год издания: ${book.Year}</p>" +
                   "<p>Автор: ${book.Author}</p>";
Book book = new Book("УХвздывж", 1234,"УНТ");

string result1 = htr.RenderFromString(template1, book);
Console.WriteLine(result1);

string template2 = "<p>Название ручки: ${pen.Title}</p>" +
                   "<p>Цвет: ${pen.Color}</p>" +
                   "<p>Толщина: ${pen.Width}</p>" +
                   "<p>Цена: ${pen.Price}</p>";
Pen pen = new Pen("Brauberg", "Green",25, 123);

string result2 = htr.RenderFromString(template2, pen);
Console.WriteLine(result2);

string t3 = "<p>Имя: ${user.Name}</p>" +
            "<p>Пол: $if ${user.Sex}мужской$else женский$endif</p>";
Console.WriteLine(htr.RenderFromString(t3, user));
string t4 = "<p>Возраст: ${user.Age}</p>" +
            "$if ${user.Age} >= 18" +
            "<p>Совершеннолетний</p>" +
            "$else" +
            "<p>Несовершеннолетний</p>" +
            "$endif";
Console.WriteLine(htr.RenderFromString(t4, user));

var shop = new Shop(
    new List<Pen>
    {
        new Pen("Brauberg", "Green", 25, 123),
        new Pen("Pilot",    "Blue",  10, 89),
        new Pen("ErichKrause","Red", 15, 150)
    },
    new List<User> { user, new User("Петя", 15, "11-541", false) });

string t5 = "<ul>" +
            "$foreach ${p} in ${shop.Pens}" +
            "<li>${p.Title} — ${p.Color}, ${p.Width} мм, ${p.Price} ₽</li>" +
            "$endfor" +
            "</ul>";
Console.WriteLine(htr.RenderFromString(t5, shop));

string t6 = "<ul>" +
            "$foreach ${u} in ${shop.Users}" +
            "<li>${u.Name}" +
            "$if ${u.Sex}" +
            " — $if ${u.Age} >= 18 взрослый$else мальчик$endif" +
            "$else" +
            " — $if ${u.Age} >= 18 взрослая$else девочка$endif" +
            "$endif" +
            "</li>" +
            "$endfor" +
            "</ul>";
Console.WriteLine(htr.RenderFromString(t6, shop));