namespace NewConsoleApp;

public class Shop
{
    List<Pen> Pens { get; set; }
    List<User> Users { get; set; }

    public Shop(List<Pen> pens, List<User> users)
    {
        Pens = pens;
        Users = users;
    }
}