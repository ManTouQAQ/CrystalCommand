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
        var result = await arguments[key].GetResultAsync();
        return (T?)result;
    }
}