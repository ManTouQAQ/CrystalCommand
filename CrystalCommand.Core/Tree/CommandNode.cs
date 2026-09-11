namespace CrystalCommand.Core.Tree;

public abstract class CommandNode<TCommandSender>
{
    private readonly List<LiteralCommandNode<TCommandSender>> _literalChildren = [];
    private readonly List<ArgumentCommandNode<TCommandSender>> _argumentChildren = [];

    public IReadOnlyList<LiteralCommandNode<TCommandSender>> LiteralChildren => _literalChildren;
    public IReadOnlyList<ArgumentCommandNode<TCommandSender>> ArgumentChildren => _argumentChildren;

    public Func<CommandContext<TCommandSender>, Task<bool>>? Handler { get; set; }
    public Func<ParseContext<TCommandSender>, RequirementCheckResult>? Requirement { get; set; }
    public abstract string Key { get; }

    public TNode AddChild<TNode>(TNode child)
        where TNode : CommandNode<TCommandSender>
    {
        switch (child)
        {
            case LiteralCommandNode<TCommandSender> literalChild:
                if (_literalChildren.Any(x => x.Key == literalChild.Key))
                    throw new ArgumentException(
                        $"A literal command node with key '{literalChild.Key}' already exists.");
                _literalChildren.Add(literalChild);
                break;
            case ArgumentCommandNode<TCommandSender> argumentChild:
                if (_argumentChildren.Any(x => x.Key == argumentChild.Key))
                    throw new ArgumentException(
                        $"An argument command node with key '{argumentChild.Key}' already exists.");

                _argumentChildren.Add(argumentChild);
                break;
            default:
                throw new ArgumentException($"Unsupported command node type: {child.GetType().Name}");
        }

        return child;
    }

    public void ParseNode(ParseContext<TCommandSender> context)
    {
        ParseNode0(context, this);
    }

    protected virtual void ParseNode0(ParseContext<TCommandSender> context, CommandNode<TCommandSender> upstreamNode)
    {
        if (!context.LineReader.HasMore)
        {
            context.CommitResult(upstreamNode);
            return;
        }

        foreach (var node in _literalChildren)
        {
            node.ParseNode0(context, upstreamNode);
        }

        foreach (var node in _argumentChildren)
        {
            node.ParseNode0(context, upstreamNode);
        }
    }

    protected bool CheckRequirement(ParseContext<TCommandSender> context)
    {
        if (Requirement == null) return true;

        var result = Requirement.Invoke(context);
        if (result.Passed) return true;
        context.CommitFailureResult(this, result.Exception!);
        return false;
    }
}

public readonly struct RequirementCheckResult(
    bool passed,
    Exception? exception
)
{
    public bool Passed { get; } = passed;
    public Exception? Exception { get; } = exception;

    public static RequirementCheckResult Pass()
    {
        return new(true, null);
    }

    public static RequirementCheckResult Failure(Exception? exception = null)
    {
        return new(false, exception ?? new RequirementCheckException());
    }
    
    public static RequirementCheckResult Failure(string msg)
    {
        return new(false, new RequirementCheckException(msg));
    }
}

public class RequirementCheckException(string? msg = null) : Exception(msg ?? "Requirement check failed");