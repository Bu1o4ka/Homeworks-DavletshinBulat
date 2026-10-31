using ControlWork;

var server = new HttpServer();
_ = server.Start();
Console.WriteLine("Сервер запущен");
while (true)
{
    if (Console.ReadLine() == "stop")
    {
        break;
    }
}
server.Stop();
Console.WriteLine("Сервер остановлен");