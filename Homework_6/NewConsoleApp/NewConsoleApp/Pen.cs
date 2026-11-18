namespace NewConsoleApp;

public class Pen
{
    string Title{get;set;}
    string Color{get;set;}
    int Width{get;set;}
    decimal Price{get;set;}

    public Pen(string title, string color, int width, decimal price)
    {
        Title = title;
        Color = color;
        Width = width;
        Price = price;
    }
}