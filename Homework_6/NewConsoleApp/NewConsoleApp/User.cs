namespace NewConsoleApp;

public class User
{
    string Name{get;set;}
    int Age {get;set;}
    string Group{get;set;}
    bool Sex{get;set;}

    public User(string name, int age, string group, bool sex)
    {
        Name = name;
        Age = age;
        Group = group;
        Sex = sex;
    }
}