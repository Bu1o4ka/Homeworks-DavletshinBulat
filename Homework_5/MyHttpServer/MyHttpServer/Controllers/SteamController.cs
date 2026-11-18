// using MyHttpServer.Framework;
// using MyHttpServer.Framework.Attributes;
// using MailKit.Net.Smtp;
// using MailKit.Security;
// using MimeKit;
//
// namespace MyHttpServer.Contollers;
// [HttpController("steam")]
// public class SteamController: ControllerBase
// {
//     
//
//     [Get("send")]
//     public async Task<string> Send(string to)
//     {
//         try
//         {
//             var message = new MimeMessage();
//             message.From.Add(new MailboxAddress("Bulat", "latiake@yandex.ru"));
//             message.To.Add(new MailboxAddress("", to));
//             message.Subject = "Тест";
//             message.Body = new TextPart("html") { Text = "<h2>Письмо-тест</h2>" };
//
//             using var client = new SmtpClient();
//             await client.ConnectAsync("smtp.yandex.ru", 465, SecureSocketOptions.SslOnConnect);
//             await client.AuthenticateAsync("latiake@yandex.ru", "oxdbkdpftbbpcdgq");
//             await client.SendAsync(message);
//             await client.DisconnectAsync(true);
//
//             return "Письмо отправлено";
//         }
//         catch (Exception ex)
//         {
//             return $"Ошибка: {ex.Message}";
//         }
//     }
//     //GET https://localhost:2323/steam/send?to=test@test.ru&hero=abaddon
//     // Герой: Abaddon
//     // 1. Dreadmist Dragon - Mount (Аксессуар)
//     // 2.
//     // [Get("send")]
//     // public string Send(string to)
//     // {
//     //     try
//     //     {
//     //         // отправитель - устанавливаем адрес и отображаемое в письме имя
//     //         MailAddress from = new MailAddress("latiake@yandex.ru", "Bulat");
//     //         // кому отправляем
//     //         MailAddress toAddress = new MailAddress("latiake@yandex.ru");
//     //         // создаем объект сообщения
//     //         MailMessage m = new MailMessage(from, toAddress);
//     //         // тема письма
//     //         m.Subject = "Тест";
//     //         // текст письма
//     //         m.Body = "<h2>Письмо-тест работы smtp-клиента</h2>";
//     //         // письмо представляет код html
//     //         m.IsBodyHtml = true;
//     //         // адрес smtp-сервера и порт, с которого будем отправлять письмо
//     //         SmtpClient smtp = new SmtpClient("smtp.yandex.ru", 587);
//     //         // логин и пароль
//     //         smtp.Credentials = new NetworkCredential("latiake@yandex.ru", "ipudntnbceczrvav");
//     //         smtp.EnableSsl = true;
//     //         smtp.UseDefaultCredentials = false;
//     //         smtp.Send(m);
//     //         return $"Письмо отправлено на {to}";
//     //     }
//     //     catch (Exception ex)
//     //     {
//     //         return $"Ошибка: {ex.Message}";
//     //     }
//     // }
//     [Get("")]
//     public string Index()
//     {
//         string path = Path.Combine(Directory.GetCurrentDirectory(), "static", "Steam", "login.html");
//         if (!File.Exists(path))
//             path = Path.Combine(AppContext.BaseDirectory, "static", "Steam", "login.html");
//         return File.ReadAllText(path);
//     }
// }