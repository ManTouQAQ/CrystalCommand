using CrystalCommand.Core.Argument;

namespace CrystalCommand.Core;

public class CommandContext<TCommandSender>(
    TCommandSender sender,
    IReadOnlyDictionary<string, ArgumentParseResult> arguments
)
{
    public TCommandSender CommandSender { get; } = sender;

    public async Task<T?> GetArgumentAsync<T>(string key)
    {
        if (!arguments.TryGetValue(key, out var argument)) return default;
        var result = await argument.GetResultAsync();
        return (T?)result;
    }
}