namespace CrystalCommand.Core.Tree;

public class LiteralCommandNode<TCommandSender>(
    string key
) : CommandNode<TCommandSender>
{
    public override string Key { get; } = key;

    public override void ParseNode(ParseContext<TCommandSender> context)
    {
        var reader = context.LineReader;
        var pointerSnapshot = reader.Pointer;
        var command = reader.ReadNext();

        if (Key != command || !CheckRequirement(context))
        {
            context.LineReader.Pointer = pointerSnapshot;
            return;
        }
        
        base.ParseNode(context);
        context.LineReader.Pointer = pointerSnapshot;
    }
}