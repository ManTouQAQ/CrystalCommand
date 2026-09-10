using System.Numerics;

namespace CrystalCommand.Core.Argument;

public abstract class NumberArgumentParser<TCommandSender, T> : IArgumentParser<TCommandSender, T>
    where T : struct, INumber<T>
{
    private T? _min;
    private T? _max;

    public T? Min
    {
        get => _min;
        set
        {
            if (value.HasValue && _max.HasValue && value.Value > _max.Value)
                throw new ArgumentException("Minimum value cannot be greater than maximum value.");

            _min = value;
        }
    }

    public T? Max
    {
        get => _max;
        set
        {
            if (value.HasValue && _min.HasValue && value.Value < _min.Value)
                throw new ArgumentException("Maximum value cannot be less than minimum value.");

            _max = value;
        }
    }

    public bool Clamp { get; set; } = false;

    public ArgumentParseResult<T> Parse(ParseContext<TCommandSender> context)
    {
        var reader = context.LineReader;

        var text = reader.ReadNext()!;

        if (!T.TryParse(text, null, out var value))
        {
            return FailureResult(text, NumberParseException.ErrorType.NotANumber);
        }

        value = ProcessValue(value);
        if (value < Min)
        {
            if (Clamp)
            {
                value = Min.Value;
            }
            else
            {
                return FailureResult(text, NumberParseException.ErrorType.TooSmall);
            }
        }

        if (value > Max)
        {
            if (Clamp)
            {
                value = Max.Value;
            }
            else
            {
                return FailureResult(text, NumberParseException.ErrorType.TooLarge);
            }
        }

        return ArgumentParseResult<T>.Success(value);
    }

    protected virtual T ProcessValue(T value)
    {
        return value;
    }

    private ArgumentParseResult<T> FailureResult(string src, NumberParseException.ErrorType type)
    {
        var message = type switch
        {
            NumberParseException.ErrorType.NotANumber => null,
            NumberParseException.ErrorType.TooSmall => $"\"{src}\" is too small than {Min}",
            NumberParseException.ErrorType.TooLarge => $"\"{src}\" is too large than {Max}",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

        return ArgumentParseResult<T>.Failure(
            new NumberParseException(src, typeof(T), message, type)
        );
    }
}

public class NumberParseException(
    string src,
    Type type,
    string? msg,
    NumberParseException.ErrorType errorType
) : ArgumentParseException(type, src, msg)
{
    public ErrorType Type { get; } = errorType;

    public enum ErrorType
    {
        NotANumber,
        TooSmall,
        TooLarge
    }
}

public class IntArgumentParser<TCommandSender> : NumberArgumentParser<TCommandSender, int>;

public class LongArgumentParser<TCommandSender> : NumberArgumentParser<TCommandSender, long>;

public abstract class FloatingPointArgumentParser<TCommandSender, T> : NumberArgumentParser<TCommandSender, T>
    where T : struct, IFloatingPoint<T>
{
    public int? DecimalPlaces { get; set; }

    protected override T ProcessValue(T value)
    {
        if (!DecimalPlaces.HasValue) return value;

        var factor = T.CreateChecked(Math.Pow(10, DecimalPlaces.Value));
        return T.Truncate(value * factor) / factor;
    }
}

public class FloatArgumentParser<TCommandSender> : FloatingPointArgumentParser<TCommandSender, float>;

public class DoubleArgumentParser<TCommandSender> : FloatingPointArgumentParser<TCommandSender, double>;