using CrystalCommand.Core.Tree;

namespace CrystalCommand.Core;

public class CommandDispatcher<TCommandSender>
{
    private readonly List<LiteralCommandNode<TCommandSender>> _commands = [];

    public void RegisterCommand(LiteralCommandNode<TCommandSender> command)
    {
        _commands.Add(command);
    }

    public ParseContext<TCommandSender> Dispatch(TCommandSender sender, string command)
    {
        var context = new ParseContext<TCommandSender>(sender, command);

        foreach (var node in _commands)
        {
            node.ParseNode(context);
        }
        
        return context;
    }
}