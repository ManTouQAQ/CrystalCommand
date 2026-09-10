using CrystalCommand.Core.Argument;
using CrystalCommand.Core.Tree;

namespace CrystalCommand.Core;

public class CommandBuilder
{
    public static LiteralCommandNode<TCommandSender> Command<TCommandSender>(
        string key,
        Func<CommandContext<TCommandSender>, Task<bool>>? handler = null,
        Func<ParseContext<TCommandSender>, RequirementCheckResult>? requirement = null,
        Action<NodeBuilder<TCommandSender>>? configure = null
    )
    {
        var node = new LiteralCommandNode<TCommandSender>(key)
        {
            Handler = handler,
            Requirement = requirement
        };

        configure?.Invoke(new NodeBuilder<TCommandSender>(node));
        return node;
    }

    public static LiteralCommandNode<TCommandSender> Command<TCommandSender>(
        string key,
        Func<CommandContext<TCommandSender>, bool> handler,
        Func<ParseContext<TCommandSender>, RequirementCheckResult>? requirement = null,
        Action<NodeBuilder<TCommandSender>>? configure = null
    )
    {
        return Command(
            key,
            context => Task.FromResult(handler(context)),
            requirement,
            configure
        );
    }
}

public class NodeBuilder<TCommandSender>(
    CommandNode<TCommandSender> node
)
{
    public LiteralCommandNode<TCommandSender> Literal(
        string key,
        Func<CommandContext<TCommandSender>, Task<bool>>? handler = null,
        Func<ParseContext<TCommandSender>, RequirementCheckResult>? requirement = null,
        Action<NodeBuilder<TCommandSender>>? configure = null
    )
    {
        var child = node.AddChild(
            new LiteralCommandNode<TCommandSender>(key)
            {
                Handler = handler,
                Requirement = requirement
            }
        );

        configure?.Invoke(new NodeBuilder<TCommandSender>(child));
        return child;
    }

    public LiteralCommandNode<TCommandSender> Literal(
        string key,
        Func<CommandContext<TCommandSender>, bool> handler,
        Func<ParseContext<TCommandSender>, RequirementCheckResult>? requirement = null,
        Action<NodeBuilder<TCommandSender>>? configure = null
    )
    {
        return Literal(
            key,
            context => Task.FromResult(handler(context)),
            requirement,
            configure
        );
    }

    public ArgumentCommandNode<TCommandSender> Argument(
        string key,
        IArgumentParser<TCommandSender> parser,
        Func<CommandContext<TCommandSender>, Task<bool>>? handler = null,
        Func<ParseContext<TCommandSender>, RequirementCheckResult>? requirement = null,
        Action<NodeBuilder<TCommandSender>>? configure = null
    )
    {
        var child = node.AddChild(
            new ArgumentCommandNode<TCommandSender>(key, parser)
            {
                Handler = handler,
                Requirement = requirement
            }
        );

        configure?.Invoke(new NodeBuilder<TCommandSender>(child));
        return child;
    }

    public ArgumentCommandNode<TCommandSender> Argument(
        string key,
        IArgumentParser<TCommandSender> parser,
        Func<CommandContext<TCommandSender>, bool> handler,
        Func<ParseContext<TCommandSender>, RequirementCheckResult>? requirement = null,
        Action<NodeBuilder<TCommandSender>>? configure = null
    )
    {
        return Argument(
            key,
            parser,
            context => Task.FromResult(handler(context)),
            requirement,
            configure
        );
    }
}