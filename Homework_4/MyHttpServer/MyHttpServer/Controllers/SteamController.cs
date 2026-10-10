using System.Net;
using System.Net.Mail;
using MyHttpServer.Framework;
using MyHttpServer.Framework.Attributes;

/*namespace MyHttpServer.Contollers;
[HttpController("steam")]*/
/*public class SteamController: ControllerBase
{
    //GET https://localhost:2323/steam/send?to=test@test.ru&hero=abaddon
    // Герой: Abaddon
    // 1. Dreadmist Dragon - Mount (Аксессуар)
    // 2.
    [Get("send")]
    public void Send([FromQuery] string to, string heroes)
    {
        // отправитель - устанавливаем адрес и отображаемое в письме имя
        MailAddress from = new MailAddress("somemail@gmail.com", "Tom");
        // кому отправляем
        MailAddress toAddress = new MailAddress("somemail@yandex.ru");
        // создаем объект сообщения
        MailMessage m = new MailMessage(from, toAddress);
        // тема письма
        m.Subject = "Тест";
        // текст письма
        m.Body = "<h2>Письмо-тест работы smtp-клиента</h2>";
        // письмо представляет код html
        m.IsBodyHtml = true;
        // адрес smtp-сервера и порт, с которого будем отправлять письмо
        SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587);
        // логин и пароль
        smtp.Credentials = new NetworkCredential("somemail@gmail.com", "mypassword");
        smtp.EnableSsl = true;
        smtp.Send(m);
        Console.Read();
    }
}*/