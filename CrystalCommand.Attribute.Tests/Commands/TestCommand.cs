using CrystalCommand.Attribute.Arguments;
using CrystalCommand.Attribute.Attribute;

namespace CrystalCommand.Attribute.Tests.Commands;

[Command("test")]
public class TestCommand
{
    [Command]
    public void Root(TestSender sender)
    {
        sender.Result = "test";
    }

    [Command("foo1")]
    public void Foo1(TestSender sender)
    {
        sender.Result = "test foo1";
    }

    [Command("foo1 bar1")]
    public void Bar1(TestSender sender)
    {
        sender.Result = "test foo1 bar1";
    }

    [Command("foo1 <method> [number1] [number2]")]
    public void ArgsTest(
        TestSender sender,
        string method,
        [Arg("number2")] int n,
        [Arg] int number1
    )
    {
        sender.Result = $"test foo1 {method} {number1} {n}";
    }
    
    [Command("range <number1> <number2>")]
    public void RangeTest(
        TestSender sender, 
        [Min(1)] int number1,
        [Min(1)] [Max(4)] [Clamp] int number2
    )
    {
        sender.Result = $"range {number1} {number2}";
    }

    [Command("quoted [msg]")]
    public void OptionTest(TestSender sender, [Quoted] string? msg)
    {
        sender.Result = $"test quoted > {msg ?? "null"}";
    }

    [Command("admin")]
    [Permission("admin")]
    public void Admin(TestSender sender)
    {
        sender.Result = "admin";
    }

    [Command("admin <method>")]
    [Permission("admin.crud")]
    public void AdminMethod(TestSender sender, string method)
    {
        sender.Result = $"admin {method}";
    }

    [Command("admin settings [name]")]
    [Permission("admin.settings")]
    public void AdminSettings(TestSender sender, string name)
    {
        sender.Result = $"admin settings {name}";
    }
}