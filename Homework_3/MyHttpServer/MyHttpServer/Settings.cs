public class Settings
{
    public Server Server { get; set; } = new();
}

public class Server
{
    public string Port { get; set; } = "8888";
    public string Host { get; set; } = "localhost";
    public string Path { get; set; } = "";
}