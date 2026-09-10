namespace CrystalCommand.Core.Tree;

public abstract class CommandNode<TCommandSender>
{
    private List<LiteralCommandNode<TCommandSender>> _literalChildren = [];
    private List<ArgumentCommandNode<TCommandSender>> _argumentChildren = [];

    public Func<CommandContext<TCommandSender>, Task<bool>>? Handler { get; set; }
    public Func<ParseContext<TCommandSender>, RequirementCheckResult>? Requirement { get; set; }
    public abstract string Key { get; }

    public TNode AddChild<TNode>(TNode child)
        where TNode : CommandNode<TCommandSender>
    {
        switch (child)
        {
            case LiteralCommandNode<TCommandSender> literalChild:
                _literalChildren.Add(literalChild);
                break;
            case ArgumentCommandNode<TCommandSender> argumentChild:
                _argumentChildren.Add(argumentChild);
                break;
            default:
                throw new ArgumentException($"Unsupported command node type: {child.GetType().Name}");
        }

        return child;
    }

    public virtual void ParseNode(ParseContext<TCommandSender> context)
    {
        if (!context.LineReader.HasMore)
        {
            context.CommitResult(this);
            return;
        }

        foreach (var node in _literalChildren)
        {
            node.ParseNode(context);
        }

        foreach (var node in _argumentChildren)
        {
            node.ParseNode(context);
        }
    }

    public bool CheckRequirement(ParseContext<TCommandSender> context)
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
}

public class RequirementCheckException(string? msg = null) : Exception(msg ?? "Requirement check failed");