using CrystalCommand.Attribute.Tests.Commands;
using CrystalCommand.Core;
using CrystalCommand.Core.Tree;

namespace CrystalCommand.Attribute.Tests;

public class AttributeCommandParserTest
{
    private readonly CommandManager<TestSender> _commandManager = new();

    public AttributeCommandParserTest()
    {
        var parser = new AttributeCommandParser<TestSender>("tester", _commandManager);
        parser.RegisterRequirement(new PermissionRequirementFactory());
        parser.ParseCommand(new TestCommand());
    }

    [Theory]
    [InlineData("test", "test")]
    [InlineData("test foo1", "test foo1")]
    [InlineData("test foo1 bar1", "test foo1 bar1")]
    [InlineData("test foo1 qwe", "test foo1 qwe 0 0")]
    [InlineData("test foo1 qwe 2 3", "test foo1 qwe 2 3")]
    [InlineData("test quoted", "test quoted > null")]
    [InlineData("test quoted qwe qwe", null)]
    [InlineData("test quoted 'qwe qwe'", "test quoted > qwe qwe")]
    [InlineData("test admin", null)]
    [InlineData("test admin add", null)]
    [InlineData("test admin settings", null)]
    [InlineData("test admin settings online", null)]
    [InlineData("test admin", "admin", new[] { "admin.*" })]
    [InlineData("test admin", null, new[] { "admin.crud" })]
    [InlineData("test admin add", null, new[] { "admin.crud" })]
    [InlineData("test admin add", "admin add", new[] { "admin", "admin.crud" })]
    [InlineData("test admin settings", "admin settings ", new[] { "admin", "admin.settings" })]
    [InlineData("test range 0 1", null)]
    [InlineData("test range 1 1", "range 1 1")]
    [InlineData("test range 1 5", "range 1 4")]
    public async Task Test(string command, string? expected, string[]? permissions = null)
    {
        permissions ??= [];
        var sender = new TestSender([.. permissions]);
        await _commandManager.ExecuteAsync(sender, command);

        Assert.Equal(expected, sender.Result);
    }
}

public class TestSender(List<string> permissions)
{
    public string? Result { get; set; }
    public List<string> Permissions { get; } = permissions;

    public bool HasPermission(string permission)
    {
        return Permissions.Any(granted =>
        {
            if (granted == "*" || granted == permission) return true;

            if (!granted.EndsWith(".*")) return false;

            var prefix = granted[..^1];
            return permission == prefix[..^1] || permission.StartsWith(prefix);
        });
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class PermissionAttribute(string permission) : System.Attribute
{
    public string Permission { get; } = permission;
}

public class PermissionRequirementFactory : IRequirementFactory<TestSender, PermissionAttribute>
{
    public Func<ParseContext<TestSender>, RequirementCheckResult> Create(PermissionAttribute attribute)
    {
        return c => c.CommandSender.HasPermission(attribute.Permission)
            ? RequirementCheckResult.Pass()
            : RequirementCheckResult.Failure($"do not have permission: {attribute.Permission}");
    }
}