using Microsoft.VisualStudio.TestTools.UnitTesting;
using MiniTemplateEngine;

namespace MyHttpServer.Tests;

public class TestUser 
{ 
    public string Name { get; set; } = ""; 
    public int Age { get; set; } 
    public bool Sex { get; set; } 
    public TestUser? Friend { get; set; } 
}

public class TestShop 
{ 
    public List<TestUser> Users { get; set; } = new(); 
    public List<TestUser>? NullUsers { get; set; } 
}

[TestClass]
public class TemplateEngineTests
{
    private HtmlTemplateRenderer _renderer = new();

    [DataTestMethod]
    [DataRow("${Name}", "Ivan", "Ivan")]
    [DataRow("Hello, ${Name}!", "Ivan", "Hello, Ivan!")]
    [DataRow("${name}", "Ivan", "Ivan")]
    [DataRow("${Unknown}", "Ivan", "")]
    [DataRow("${Friend.Name}", "Ivan", "")]
    public void Variables(string template, string name, string expected)
    {
        var model = new TestUser { Name = name };
        var result = _renderer.RenderFromString(template, model);
        Assert.IsTrue(result == expected);
    }

    [DataTestMethod]
    [DataRow("$if ${Age} > 18 Adult $endif", 20, " Adult ")]
    [DataRow("$if ${Age} > 18 Adult $endif", 18, "")]
    [DataRow("$if ${Age} < 18 Child $endif", 10, " Child ")]
    [DataRow("$if ${Age} == 18 Exact $endif", 18, " Exact ")]
    [DataRow("$if ${Age} != 18 NotExact $endif", 20, " NotExact ")]
    [DataRow("$if ${Age} >= 18 Adult $else Child $endif", 18, " Adult ")]
    [DataRow("$if ${Age} <= 17 Child $else Adult $endif", 18, " Adult ")]
    [DataRow("$if ${Name} == 'Ivan' Yes $endif", "Ivan", " Yes ")]
    [DataRow("$if ${Name} == 'Petr' No $else Yes $endif", "Ivan", " Yes ")]
    [DataRow("$if ${Sex} Male $else Female $endif", true, " Male ")]
    [DataRow("$if ${Sex} Male $else Female $endif", false, " Female ")]
    public void Conditions(string template, object value, string expected)
    {
        var model = new TestUser();
        if (value is int i) model.Age = i;
        else if (value is string s) model.Name = s;
        else if (value is bool b) model.Sex = b;

        var result = _renderer.RenderFromString(template, model);
        Assert.IsTrue(result.Trim() == expected.Trim());
    }

    [TestMethod]
    public void Foreach_EmptyList()
    {
        var result = _renderer.RenderFromString("$foreach ${u} in ${Users}${u.Name}$endfor", new TestShop());
        Assert.IsTrue(result == "");
    }

    [TestMethod]
    public void Foreach_NullList()
    {
        var result = _renderer.RenderFromString("$foreach ${u} in ${NullUsers}${u.Name}$endfor", new TestShop());
        Assert.IsTrue(result == "");
    }

    [TestMethod]
    public void Foreach_OneItem()
    {
        var model = new TestShop { Users = new() { new() { Name = "A" } } };
        var result = _renderer.RenderFromString("$foreach ${u} in ${Users}${u.Name}$endfor", model);
        Assert.IsTrue(result == "A");
    }

    [TestMethod]
    public void Foreach_ManyItems()
    {
        var model = new TestShop { Users = new() { new() { Name = "A" }, new() { Name = "B" }, new() { Name = "C" } } };
        var result = _renderer.RenderFromString("$foreach ${u} in ${Users}${u.Name}$endfor", model);
        Assert.IsTrue(result == "ABC");
    }

    [TestMethod]
    public void Nested_IfInsideForeach()
    {
        var model = new TestShop { Users = new() { new() { Name = "A", Age = 20 }, new() { Name = "B", Age = 10 } } };
        string t = "$foreach ${u} in ${Users}$if ${u.Age} >= 18 [${u.Name}] $endif$endfor";
        var result = _renderer.RenderFromString(t, model);
        Assert.IsTrue(result.Trim() == "[A]");
    }
    
    [TestMethod]
    public void Nested_ForeachInsideIf()
    {
        var model = new TestShop { Users = new() { new() { Name = "A" }, new() { Name = "B" } } };
        string t = "$if ${Users} $foreach ${u} in ${Users}${u.Name}$endfor $endif";
        var result = _renderer.RenderFromString(t, model);
        Assert.IsTrue(result == " AB ");
    }
}