using System.Reflection;
using CrystalCommand.Core.Argument;

namespace CrystalCommand.Attribute;

public interface IArgumentParserFactory<TCommandSender>
{
    IArgumentParser<TCommandSender> Create(ParameterInfo parameter);
}