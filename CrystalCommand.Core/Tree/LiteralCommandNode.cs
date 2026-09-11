namespace CrystalCommand.Core.Tree;

public class LiteralCommandNode<TCommandSender>(
    string key
) : CommandNode<TCommandSender>
{
    public override string Key { get; } = key;

    protected override void ParseNode0(ParseContext<TCommandSender> context, CommandNode<TCommandSender> upstreamNode)
    {
        var reader = context.LineReader;
        var pointerSnapshot = reader.Pointer;
        var command = reader.ReadNext();

        if (Key != command || !CheckRequirement(context))
        {
            context.LineReader.Pointer = pointerSnapshot;
            return;
        }
        base.ParseNode0(context, this);
        context.LineReader.Pointer = pointerSnapshot;
    }
}