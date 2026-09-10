namespace CrystalCommand.Core.Argument;

public interface IArgumentParser<TCommandSender>
{
    ArgumentParseResult Parse(
        ParseContext<TCommandSender> context
    );
}

public interface IArgumentParser<TCommandSender, TResult> : IArgumentParser<TCommandSender>
{
    new ArgumentParseResult<TResult> Parse(ParseContext<TCommandSender> context);

    ArgumentParseResult IArgumentParser<TCommandSender>.Parse(ParseContext<TCommandSender> context)
        => Parse(context);
}

public class ArgumentParseResult(
    bool isSuccess,
    Lazy<Task<object?>> value,
    Exception? exception
)
{
    public bool IsSuccess { get; } = isSuccess;
    public Exception? Exception { get; } = exception;
    
    public Task<object?> GetResultAsync() => value.Value;
}

public class ArgumentParseResult<TResult>(
    bool isSuccess,
    Lazy<Task<TResult?>> value,
    Exception? exception
) : ArgumentParseResult(
    isSuccess,
    new Lazy<Task<object?>>(async () => await value.Value),
    exception
)
{
    public static ArgumentParseResult<TResult> Success(Func<Task<TResult?>> value)
    {
        return new(
            true,
            new Lazy<Task<TResult?>>(value),
            null
        );
    }

    public static ArgumentParseResult<TResult> Success(TResult? value)
    {
        return Success(() => Task.FromResult(value));
    }

    public static ArgumentParseResult<TResult> Failure(Exception? exception = null)
    {
        return new ArgumentParseResult<TResult>(
            false,
            new Lazy<Task<TResult?>>(() => Task.FromResult<TResult?>(default)),
            exception
        );
    }
}

public class ArgumentParseException(Type type, string src, string? message = null)
    : Exception(message ?? $"\"{src}\" is not a \"{type}\"");