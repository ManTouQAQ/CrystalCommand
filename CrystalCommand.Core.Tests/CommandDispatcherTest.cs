namespace CrystalCommand.Core.Tests;

public class CommandDispatcherTest
{
    private readonly CommandDispatcher<object> _commandDispatcher = new();

    public CommandDispatcherTest()
    {
        _commandDispatcher.RegisterCommand(
            CommandBuilder.Command<object>("foo1")
        );
        _commandDispatcher.RegisterCommand(
            CommandBuilder.Command<object>("foo2")
        );
    }

    [Theory]
    [InlineData("foo1")]
    [InlineData("foo2")]
    public void DispatchTest(string input)
    {
        var context = _commandDispatcher.Dispatch(new object(), input);
        Assert.Single(context.ParsedResults);
        Assert.Equal(input, context.ParsedResults.First().ReachedNode.Key);
    }
}