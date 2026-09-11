using System.Reflection;
using CrystalCommand.Core;
using CrystalCommand.Core.Tree;

namespace CrystalCommand.Attribute;

public interface IRequirementFactory<TCommandSender>
{
    Func<ParseContext<TCommandSender>, RequirementCheckResult> Create(System.Attribute attribute);
}

public interface IRequirementFactory<TCommandSender, in TAttribute> : IRequirementFactory<TCommandSender>
    where TAttribute : System.Attribute
{
    Func<ParseContext<TCommandSender>, RequirementCheckResult> IRequirementFactory<TCommandSender>.Create(
        System.Attribute attribute)
        => Create((TAttribute)attribute);

    Func<ParseContext<TCommandSender>, RequirementCheckResult> Create(TAttribute attribute);
}