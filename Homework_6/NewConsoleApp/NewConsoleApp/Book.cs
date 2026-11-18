namespace NewConsoleApp;

public class Book
{
    string Title{get;set;}
    int Year{get;set;}
    string Author{get;set;}

    public Book(string title, int year, string author)
    {
        Title = title;
        Year = year;
        Author = author;
    }
    
}