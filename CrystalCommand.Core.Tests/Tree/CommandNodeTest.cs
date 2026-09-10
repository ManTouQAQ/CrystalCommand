using System.Diagnostics.CodeAnalysis;
using CrystalCommand.Core.Argument;
using CrystalCommand.Core.Tree;

namespace CrystalCommand.Core.Tests.Tree;

public class CommandNodeTest
{
    private string? _result;
    private readonly LiteralCommandNode<TestSender> _root;

    [SuppressMessage("ReSharper", "VariableHidesOuterVariable")]
    public CommandNodeTest()
    {
        var checkPermission = (ParseContext<TestSender> c) =>
            c.CommandSender.HasPermissions ? RequirementCheckResult.Pass() : RequirementCheckResult.Failure();
        
        _root = CommandBuilder.Command<TestSender>("foo",
            _ =>
            {
                _result = "foo";
                return true;
            },
            null,
            n =>
            {
                n.Literal(
                    "bar1",
                    _ =>
                    {
                        _result = "bar1";
                        return true;
                    },
                    null,
                    n =>
                    {
                        n.Literal(
                            "admin",
                            requirement: checkPermission
                        );
                    }
                );

                n.Literal("bar2", _ =>
                {
                    _result = "bar2";
                    return true;
                });

                n.Literal("admin", requirement: checkPermission);

                n.Argument(
                    "method",
                    new StringArgumentParser<TestSender>(),
                    async c =>
                    {
                        _result = $"method-{await c.GetArgumentAsync<string>("method")}";
                        return true;
                    },
                    null,
                    n =>
                    {
                        n.Argument("number", new IntArgumentParser<TestSender>(), async c =>
                        {
                            _result =
                                $"method={await c.GetArgumentAsync<string>("method")}, number={await c.GetArgumentAsync<int>("number")}";
                            return true;
                        });
                    }
                );
            });
    }

    [Theory]
    [InlineData("foo", "foo")]
    [InlineData("foo bar1", "bar1", 2)]
    [InlineData("foo bar2", "bar2", 2)]
    [InlineData("foo hello", "method-hello")]
    [InlineData("foo hello 123", "method=hello, number=123")]
    public async Task CommandExecutionTest(string command, string expected, int expectedNode = 1)
    {
        _result = null;

        var context = new ParseContext<TestSender>(new TestSender(), command);
        _root.ParseNode(context);

        Assert.Equal(expectedNode, context.ParsedResults.Count);
        var result = context.ParsedResults.FirstOrDefault(r => r.ReachedNode.Handler != null)!;
        var commandContext = new CommandContext<TestSender>(
            context.CommandSender,
            result.Arguments
        );
        await result.ReachedNode.Handler!.Invoke(commandContext);

        Assert.Equal(expected, _result);
    }

    [Theory]
    [InlineData("foo bar1 baz")]
    [InlineData("foo hello qwe")]
    public void CommandNotFoundTest(string command)
    {
        var context = new ParseContext<TestSender>(new TestSender(), command);
        _root.ParseNode(context);
        Assert.Empty(context.ParsedResults);
    }

    [Theory]
    [InlineData("foo bar1 admin", true, 1)]
    [InlineData("foo bar1 admin", false, 0)]
    [InlineData("foo admin", true, 2)]
    [InlineData("foo admin", false, 1)]
    public void CommandRequirementTest(string command, bool hasPermissions, int expectedNode)
    {
        var context = new ParseContext<TestSender>(new TestSender(hasPermissions), command);
        _root.ParseNode(context);
        Assert.Equal(expectedNode, context.ParsedResults.Count);
    }

    [Theory]
    [InlineData("foo bar1 admin", typeof(RequirementCheckException))]
    [InlineData("foo hello a", typeof(ArgumentParseException))]
    public void CommandTrackTest(string command, Type exceptionType)
    {
        var context = new ParseContext<TestSender>(new TestSender(), command);
        _root.ParseNode(context);
        Assert.Empty(context.ParsedResults);
        Assert.NotEmpty(context.ParseFailureResults);
        Assert.IsType(exceptionType, context.ParseFailureResults.First().Value, false);
    }

    private class TestSender(bool hasPermissions = false)
    {
        public bool HasPermissions { get; } = hasPermissions;
    }
}