using System.Reflection;
using CrystalCommand.Core.Argument;

namespace CrystalCommand.Attribute.Arguments;

public class StringArgumentParserFactory<TCommandSender> : IArgumentParserFactory<TCommandSender>
{
    public IArgumentParser<TCommandSender> Create(ParameterInfo parameter)
    {
        var parser = new StringArgumentParser<TCommandSender>();
        if (parameter.IsDefined(typeof(QuotedAttribute), false))
        {
            parser.Type = StringArgumentType.Quoted;
        }
        else if (parameter.IsDefined(typeof(GreedyAttribute), false))
        {
            parser.Type = StringArgumentType.Greedy;
        }
        return parser;
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public class GreedyAttribute : System.Attribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class QuotedAttribute : System.Attribute;