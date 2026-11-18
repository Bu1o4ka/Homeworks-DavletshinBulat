using MyHttpServer.Models;

namespace MyHttpServer.ORM.Tests;

[TestClass]
public class OrmTests
{
    private const string Cs = "Server=localhost,1433;Database=OrisDb;User Id=sa;Password=P@ssw0rd;TrustServerCertificate=True;";
    private OrmContext _orm = null!;

    [TestInitialize]
    public void Setup()
    {
        _orm = new OrmContext(Cs);
    }

    [TestMethod]
    public async Task Insert_Should_Assign_Id()
    {
        var newUser = new User { Name = "TestUser", Email = "test@orm.com", Age = 99 };

        int rows = await _orm.InsertAsync("Users", newUser);

        Assert.AreEqual(1, rows);
        Assert.IsTrue(newUser.Id > 0, "ORM должен был присвоить Id после вставки");
        
        await _orm.DeleteAsync("Users", newUser.Id);
    }

    [TestMethod]
    public async Task Update_Should_Change_Data()
    {
        var user = new User { Name = "BeforeUpdate", Email = "old@orm.com", Age = 10 };
        await _orm.InsertAsync("Users", user);

        user.Name = "AfterUpdate";
        user.Age = 11;
        int rows = await _orm.UpdateAsync("Users", user.Id, user);

        Assert.AreEqual(1, rows);
        
        var allUsers = await _orm.GetAllAsync<User>("Users");
        var updatedUser = allUsers.FirstOrDefault(u => u.Id == user.Id);
        Assert.IsNotNull(updatedUser);
        Assert.AreEqual("AfterUpdate", updatedUser.Name);
        Assert.AreEqual(11, updatedUser.Age);

        // Убираем за собой
        await _orm.DeleteAsync("Users", user.Id);
    }

    [TestMethod]
    public async Task Delete_Should_Remove_Data()
    {
        var user = new User { Name = "ToDelete", Email = "delete@orm.com", Age = 5 };
        await _orm.InsertAsync("Users", user);

        int rows = await _orm.DeleteAsync("Users", user.Id);

        Assert.AreEqual(1, rows);
        
        var allUsers = await _orm.GetAllAsync<User>("Users");
        Assert.IsFalse(allUsers.Any(u => u.Id == user.Id));
    }
}