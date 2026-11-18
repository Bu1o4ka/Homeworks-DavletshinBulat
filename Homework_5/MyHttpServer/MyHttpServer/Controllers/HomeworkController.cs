using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using MyHttpServer.Framework;
using MyHttpServer.Framework.Attributes;

namespace MyHttpServer.Controllers;

[HttpController("home-work")]
public class HomeworkController : ControllerBase
{
    private const string GroupNumber = "ИС-21";
    private const string FullName = "Иванов Иван Иванович";
    private const string FromEmail = "latiake@yandex.ru";
    private const string AppPassword = "ipudntnbceczrvav";

    [Get("send")]
    public async Task<string> Send(string to)
    {
        try
        {
            string attachmentPath = Path.Combine(
                AppContext.BaseDirectory, "static", "files", "homework.zip");

            if (!File.Exists(attachmentPath))
                return $"Файл не найден: {attachmentPath}";

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(FullName, FromEmail));
            message.To.Add(new MailboxAddress("", to));
            message.Subject = $"Домашнее задание — {GroupNumber}, {FullName}";

            var body = new BodyBuilder
            {
                HtmlBody =
                    "<h3>Домашнее задание</h3>" +
                    $"<p><b>Группа:</b> {GroupNumber}</p>" +
                    $"<p><b>ФИО:</b> {FullName}</p>" +
                    "<p>Файл во вложении.</p>"
            };
            body.Attachments.Add(attachmentPath);
            message.Body = body.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync("smtp.yandex.ru", 465, SecureSocketOptions.SslOnConnect);
            await client.AuthenticateAsync(FromEmail, AppPassword);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            return $"Письмо отправлено на {to}";
        }
        catch (Exception ex)
        {
            return $"Ошибка: {ex.Message}";
        }
    }
}