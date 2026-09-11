using CrystalCommand.Core.Argument;
using CrystalCommand.Core.Tree;

namespace CrystalCommand.Core;

public class ParseContext<TCommandSender>(
    TCommandSender commandSender,
    string command
)
{
    public TCommandSender CommandSender { get; } = commandSender;
    public CommandLineReader LineReader { get; } = new(command);

    public List<ParsedResult> ParsedResults { get; } = [];
    public Dictionary<CommandNode<TCommandSender>, Exception> ParseFailureResults { get; } = new();

    private readonly Dictionary<string, ArgumentParseResult> _currentParsedArguments = [];
    
    public void CommitResult(CommandNode<TCommandSender> node)
    { 
        var result = new ParsedResult(node, new Dictionary<string, ArgumentParseResult>(_currentParsedArguments));
        ParsedResults.Add(result);
    }

    public void CommitFailureResult(CommandNode<TCommandSender> node, Exception exception)
    {
        ParseFailureResults.Add(node, exception);
    }

    public void AddArgument(string key, ArgumentParseResult value)
    {
        _currentParsedArguments.Add(key, value);
    }

    public void ClearArguments()
    {
        _currentParsedArguments.Clear();
    }
    
    public class ParsedResult(
        CommandNode<TCommandSender> node,
        Dictionary<string, ArgumentParseResult> arguments
    )
    {
        public CommandNode<TCommandSender> ReachedNode { get; } = node;
        public IReadOnlyDictionary<string, ArgumentParseResult> Arguments { get; } = arguments;
    }
}
