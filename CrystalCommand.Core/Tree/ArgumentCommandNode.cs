using CrystalCommand.Core.Argument;

namespace CrystalCommand.Core.Tree;

public class ArgumentCommandNode<TCommandSender>(
    string key,
    IArgumentParser<TCommandSender> argumentParser,
    bool optional = false
) : CommandNode<TCommandSender>
{
    public override string Key { get; } = key;
    public IArgumentParser<TCommandSender> Parser { get; } = argumentParser;
    public bool Optional { get; } = optional;

    protected override void ParseNode0(ParseContext<TCommandSender> context, CommandNode<TCommandSender> upstreamNode)
    {
        if (!CheckRequirement(context)) return;

        var pointerSnapshot = context.LineReader.Pointer;
        var result = Parser.Parse(context);

        if (!result.IsSuccess)
        {
            context.LineReader.Pointer = pointerSnapshot;
            context.CommitFailureResult(this, result.Exception!);
            return;
        }

        context.AddArgument(Key, result);
        base.ParseNode0(context, Optional ? upstreamNode : this);
        context.ClearArguments();
        context.LineReader.Pointer = pointerSnapshot;
    }
}