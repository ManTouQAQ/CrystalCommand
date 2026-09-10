using CrystalCommand.Core.Argument;

namespace CrystalCommand.Core.Tree;

public class ArgumentCommandNode<TCommandSender>(
    string key,
    IArgumentParser<TCommandSender> argumentParser
) : CommandNode<TCommandSender>
{
    public override string Key { get; } = key;
    public override void ParseNode(ParseContext<TCommandSender> context)
    {
        if (!CheckRequirement(context)) return;

        var pointerSnapshot = context.LineReader.Pointer;
        var result = argumentParser.Parse(context);

        if (!result.IsSuccess)
        {
            context.LineReader.Pointer = pointerSnapshot;
            // TODO track
            context.CommitFailureResult(this, result.Exception!);
            return;
        }
        
        context.AddArgument(Key, result);
        base.ParseNode(context);
        context.ClearArguments();
        context.LineReader.Pointer = pointerSnapshot;
    }
}