using System.Collections.Concurrent;
using CrystalCommand.Core.Tree;

namespace CrystalCommand.Core;

public class CommandManager<TCommandSender>
{
    private readonly ConcurrentDictionary<string, CommandDispatcher<TCommandSender>> _registeredCommands = new();

    public void RegisterCommand(
        string cNamespace,
        LiteralCommandNode<TCommandSender> command
    )
    {
        var dispatcher = _registeredCommands.GetOrAdd(
            cNamespace,
            _ => new CommandDispatcher<TCommandSender>()
        );

        dispatcher.RegisterCommand(command);
    }

    public async Task<ExecuteResult> ExecuteAsync(TCommandSender sender, string command, string? cNamespace = null)
    {
        if (cNamespace == null)
        {
            foreach (var dispatcher in _registeredCommands.Values)
            {
                var context = dispatcher.Dispatch(sender, command);
                var result = await ExecuteCommandAsync(context);
                if (result.Status == ExecuteStatus.CommandNotFound) continue;
                return result;
            }

            return ExecuteResult.CommandNotFound;
        }

        {
            _registeredCommands.TryGetValue(cNamespace, out var dispatcher);
            if (dispatcher == null) return ExecuteResult.CommandNotFound;
            var context = dispatcher.Dispatch(sender, command);
            return await ExecuteCommandAsync(context);
        }
    }

    private async Task<ExecuteResult> ExecuteCommandAsync(ParseContext<TCommandSender> parseContext)
    {
        if (parseContext.ParsedResults.Count == 0 && parseContext.ParseFailureResults.Count == 0) return ExecuteResult.CommandNotFound;
        
        var result = parseContext.ParsedResults.FirstOrDefault(r => r.ReachedNode.Handler != null);
        if (result == null)
        {
            if (parseContext.ParseFailureResults.Count == 0) return ExecuteResult.CommandNoHandler;

            var failureResult = parseContext.ParseFailureResults.First();
            return ExecuteResult.CommandParseFailure(failureResult.Value);
        }

        var commandContext = new CommandContext<TCommandSender>(
            parseContext.CommandSender,
            result.Arguments
        );

        return await result.ReachedNode.Handler!.Invoke(commandContext)
            ? ExecuteResult.Success
            : ExecuteResult.Failure;
    }
}

public readonly struct ExecuteResult(ExecuteStatus status, Exception? exception = null)
{
    public ExecuteStatus Status { get; } = status;
    public Exception? Exception { get; } = exception;

    public static ExecuteResult CommandNotFound => new(ExecuteStatus.CommandNotFound);

    public static ExecuteResult CommandNoHandler => new(ExecuteStatus.CommandNoHandler);
    
    public static ExecuteResult CommandParseFailure(Exception exception) =>
        new(ExecuteStatus.CommandParseFailure, exception);
    
    public static ExecuteResult Success => new(ExecuteStatus.Success);
    public static ExecuteResult Failure => new(ExecuteStatus.Failure);
}

public enum ExecuteStatus
{
    CommandNotFound,
    CommandNoHandler,
    CommandParseFailure,
    Success,
    Failure
}