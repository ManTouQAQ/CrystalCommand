using System.Numerics;
using System.Reflection;
using CrystalCommand.Core.Argument;

namespace CrystalCommand.Attribute.Arguments;

public abstract class NumberArgumentParserFactory<TCommandSender, T>(
    Func<NumberArgumentParser<TCommandSender, T>> creator
) : IArgumentParserFactory<TCommandSender>
    where T : struct, INumber<T>
{
    public virtual IArgumentParser<TCommandSender> Create(ParameterInfo parameter)
    {
        var parser = creator.Invoke();
        var min = parameter.GetCustomAttribute<MinAttribute>();
        if (min != null) parser.Min = T.CreateChecked(min.Value);
        var max = parameter.GetCustomAttribute<MaxAttribute>();
        if (max != null) parser.Max = T.CreateChecked(max.Value);
        if (parameter.IsDefined(typeof(ClampAttribute), false)) parser.Clamp = true;
        return parser;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class MinAttribute(double value) : System.Attribute
{
    public double Value { get; } = value;
}

[AttributeUsage(AttributeTargets.Parameter)]
public class MaxAttribute(double value) : System.Attribute
{
    public double Value { get; } = value;
}

[AttributeUsage(AttributeTargets.Parameter)]
public class ClampAttribute : System.Attribute;

public class IntArgumentParserFactory<TCommandSender>()
    : NumberArgumentParserFactory<TCommandSender, int>(() => new IntArgumentParser<TCommandSender>());

public class LongArgumentParserFactory<TCommandSender>()
    : NumberArgumentParserFactory<TCommandSender, long>(() => new LongArgumentParser<TCommandSender>());

[AttributeUsage(AttributeTargets.Parameter)]
public class DecimalPlacesAttribute(int value) : System.Attribute
{
    public int Value { get; } = value;
}

public abstract class FloatingPointArgumentParserFactory<TCommandSender, T>(
    Func<NumberArgumentParser<TCommandSender, T>> creator
) : NumberArgumentParserFactory<TCommandSender, T>(creator)
    where T : struct, IFloatingPoint<T>
{
    public override IArgumentParser<TCommandSender> Create(ParameterInfo parameter)
    {
        var parser = (FloatingPointArgumentParser<TCommandSender, T>)base.Create(parameter);
        var decimalPlaces = parameter.GetCustomAttribute<DecimalPlacesAttribute>();
        if (decimalPlaces != null) parser.DecimalPlaces = decimalPlaces.Value;
        return parser;
    }
}

public class FloatArgumentParserFactory<TCommandSender>()
    : FloatingPointArgumentParserFactory<TCommandSender, float>(() => new FloatArgumentParser<TCommandSender>());

public class DoubleArgumentParserFactory<TCommandSender>()
    : FloatingPointArgumentParserFactory<TCommandSender, double>(() => new DoubleArgumentParser<TCommandSender>());