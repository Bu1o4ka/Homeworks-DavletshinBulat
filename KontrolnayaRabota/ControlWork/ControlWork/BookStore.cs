namespace ControlWork;

public class BookStore
{
    private List<Book> books = new List<Book>();
    private int nextId = 1;

    public List<Book> GetAll(string author)
    {
        List<Book> result = new List<Book>();
        foreach (var b in books)
        {
            if (string.IsNullOrEmpty(author) || b.Author.ToLower().Contains(author.ToLower()))
                result.Add(b);
        }
        result.Sort((a, b) => string.Compare(a.Title, b.Title, StringComparison.OrdinalIgnoreCase));
        return result;
    }

    public Book Get(int id)
    {
        foreach (var b in books)
            if (b.Id == id) return b;
        return null;
    }

    public string Create(string title, string author)
    {
        Book b = new Book();
        b.Id = nextId;
        nextId++;
        b.Title = title;
        b.Author = author;
        b.Status = "unread";
        books.Add(b);
        return null;
    }

    public string Update(int id, string title, string author, string status)
    {
        Book b = Get(id);
        
        if (b == null) return "NOT_FOUND"; 

        if (status != "unread" && status != "read")
            return "Статус должен быть unread или read";

        b.Title = title;
        b.Author = author;
        b.Status = status;
        return null;
    }
}