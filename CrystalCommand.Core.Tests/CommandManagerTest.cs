using CrystalCommand.Core.Argument;
using CrystalCommand.Core.Tree;

namespace CrystalCommand.Core.Tests;

public class CommandManagerTest
{
    private readonly CommandManager<object> _commandManager = new();

    public CommandManagerTest()
    {
        _commandManager.RegisterCommand(
            "namespace1",
            CommandBuilder.Command<object>("foo1")
        );
        _commandManager.RegisterCommand(
            "namespace1",
            CommandBuilder.Command<object>("foo2", _ => true)
        );
        _commandManager.RegisterCommand(
            "namespace2",
            CommandBuilder.Command<object>("foo3", _ => false)
        );
        _commandManager.RegisterCommand(
            "namespace2",
            CommandBuilder.Command<object>("foo4", requirement: _ => RequirementCheckResult.Failure())
        );
        _commandManager.RegisterCommand(
            "namespace2",
            CommandBuilder.Command<object>("foo5", configure: n =>
            {
                n.Argument("number", new IntArgumentParser<object>());
            })
        );
    }

    [Theory]
    [InlineData("namespace1:foo1")]
    [InlineData("namespace1:foo3", ExecuteStatus.CommandNotFound)]
    [InlineData("namespace2:foo3")]
    [InlineData("foo1")]
    [InlineData("foo3")]
    [InlineData("foo6", ExecuteStatus.CommandNotFound)]
    public async Task NamespaceTest(string command, ExecuteStatus? executeStatus = null)
    {
        var result = await ExecuteCommandAsync(command);
        if (executeStatus != null) Assert.Equal(executeStatus, result.Status);
    }

    [Theory]
    [InlineData("foo1", ExecuteStatus.CommandNoHandler)]
    [InlineData("foo2", ExecuteStatus.Success)]
    [InlineData("foo3", ExecuteStatus.Failure)]
    [InlineData("foo4", ExecuteStatus.CommandParseFailure, typeof(RequirementCheckException))]
    [InlineData("foo5 1", ExecuteStatus.CommandNoHandler)]
    [InlineData("foo5 a", ExecuteStatus.CommandParseFailure, typeof(ArgumentParseException))]
    public async Task ExecuteReturnsTask(string command, ExecuteStatus executeStatus, Type? executeExceptionType = null)
    {
        var result = await ExecuteCommandAsync(command); 
        Assert.Equal(executeStatus, result.Status);
        if (executeExceptionType != null) Assert.IsType(executeExceptionType, result.Exception, false);
    }

    private async Task<ExecuteResult> ExecuteCommandAsync(string command)
    {
        var index = command.IndexOf(':');
        var (cNamespace, commandName) = index == -1 ? (null, command) : (command[..index], command[(index + 1)..]); 
        return await _commandManager.ExecuteAsync(
            new object(),
            commandName,
            cNamespace
        );
    }
}