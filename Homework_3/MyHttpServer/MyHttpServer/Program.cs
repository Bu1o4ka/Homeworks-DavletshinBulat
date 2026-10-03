using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using MyHttpServer;

var server = new HttpServer();
server.Start();
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




