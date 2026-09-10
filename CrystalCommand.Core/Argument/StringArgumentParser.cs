namespace CrystalCommand.Core.Argument;

public class StringArgumentParser<TCommandSender> : IArgumentParser<TCommandSender, string>
{
    public StringArgumentType Type = StringArgumentType.Normal;

    public ArgumentParseResult<string> Parse(ParseContext<TCommandSender> context)
    {
        var reader = context.LineReader;
        string? result;
        if (Type == StringArgumentType.Greedy)
        {
            result = reader.ReadGreedy();
        }
        else if (Type == StringArgumentType.Quoted)
        {
            result = reader.ReadIfQuoted();
        }
        else
        {
            result = reader.ReadNext();
        }
        return ArgumentParseResult<string>.Success(result!);
    }
}

public enum StringArgumentType
{
    Normal,
    Quoted,
    Greedy
}

public static class CommandLineRenderStringExtensions
{
    private static bool IsQuotedMark(char c) => c is '"' or '\'';

    public static string? ReadIfQuoted(this CommandLineReader reader)
    {
        while (reader.HasMore && char.IsWhiteSpace(reader.CurrentChar))
        {
            reader.Pointer++;
        }

        if (!reader.HasMore) return null;
        if (!IsQuotedMark(reader.CurrentChar)) return reader.ReadNext();

        var start = reader.Pointer;
        reader.Pointer++;
        while (reader.HasMore)
        {
            if (IsQuotedMark(reader.CurrentChar))
            {
                var end = reader.Pointer;
                reader.Pointer++;

                if (!reader.HasMore || char.IsWhiteSpace(reader.CurrentChar))
                {
                    return reader.Input[(start + 1)..end];
                }

                continue;
            }

            reader.Pointer++;
        }

        reader.Pointer = start;
        return reader.ReadNext();
    }

    public static string? ReadGreedy(this CommandLineReader reader)
    {
        while (reader.HasMore && char.IsWhiteSpace(reader.CurrentChar))
        {
            reader.Pointer++;
        }

        if (!reader.HasMore) return null;

        var result = reader.Input[reader.Pointer..];
        reader.Pointer = reader.Input.Length;
        return result;
    }
}