using System.Reflection;
using CrystalCommand.Core.Argument;

namespace CrystalCommand.Attribute.Arguments;

public class BooleanArgumentParserFactory<TCommandSender> : IArgumentParserFactory<TCommandSender>
{
    public IArgumentParser<TCommandSender> Create(ParameterInfo parameter)
    {
        return new BooleanArgumentParser<TCommandSender>();
    }
}