namespace CrystalCommand.Core.Argument;

public class BooleanArgumentParser<TCommandSender> : IArgumentParser<TCommandSender, bool>
{
    public ArgumentParseResult<bool> Parse(ParseContext<TCommandSender> context)
    {
        var reader = context.LineReader;
        var text = reader.ReadNext()!;

        if (bool.TryParse(text, out var value)) return ArgumentParseResult<bool>.Success(value);
        return ArgumentParseResult<bool>.Failure(new ArgumentParseException(typeof(bool), text));
    }
}