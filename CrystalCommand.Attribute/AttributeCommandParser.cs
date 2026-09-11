using System.Reflection;
using CrystalCommand.Attribute.Arguments;
using CrystalCommand.Attribute.Attribute;
using CrystalCommand.Core;
using CrystalCommand.Core.Argument;
using CrystalCommand.Core.Tree;

namespace CrystalCommand.Attribute;

public class AttributeCommandParser<TCommandSender>
{
    private readonly string _commandNamespace;
    private readonly CommandManager<TCommandSender> _commandManager;
    
    private readonly Dictionary<Type, IArgumentParserFactory<TCommandSender>> _argumentParserFactories = [];
    private readonly Dictionary<Type, IRequirementFactory<TCommandSender>> _requirementFactories = [];
    
    public AttributeCommandParser(
        string commandNamespace,
        CommandManager<TCommandSender> commandManager)
    {
        _commandNamespace = commandNamespace;
        _commandManager = commandManager;
        
        RegisterArgumentParser<string>(new StringArgumentParserFactory<TCommandSender>());
        RegisterArgumentParser<bool>(new BooleanArgumentParserFactory<TCommandSender>());
        RegisterArgumentParser<int>(new IntArgumentParserFactory<TCommandSender>());
        RegisterArgumentParser<long>(new LongArgumentParserFactory<TCommandSender>());
        RegisterArgumentParser<float>(new FloatArgumentParserFactory<TCommandSender>());
        RegisterArgumentParser<double>(new DoubleArgumentParserFactory<TCommandSender>());
    }

    public void RegisterArgumentParser<TArgument>(
        IArgumentParserFactory<TCommandSender> factory)
    {
        _argumentParserFactories[typeof(TArgument)] = factory;
    }

    public void RegisterRequirement<TAttribute>(
        IRequirementFactory<TCommandSender, TAttribute> factory)
        where TAttribute : System.Attribute
    {
        _requirementFactories[typeof(TAttribute)] = factory;
    }

    public void ParseCommand(object commandInstance)
    {
        _commandManager.RegisterCommand(_commandNamespace, ParseNode(commandInstance));
    }

    private LiteralCommandNode<TCommandSender> ParseNode(object commandInstance)
    {
        var type = commandInstance.GetType();

        var commandAttribute = type.GetCustomAttribute<CommandAttribute>();
        if (commandAttribute == null)
            throw new ArgumentException($"Command instance must have [{nameof(CommandAttribute)}] attribute.",
                nameof(commandInstance));
        if (commandAttribute.Route == null)
            throw new ArgumentException("Command instance's route must be assigned.", nameof(commandInstance));

        var root = ParseMemberToNode(commandInstance, commandAttribute, null, commandInstance.GetType());

        foreach (var methodInfo in type.GetMethods())
        {
            var attr = methodInfo.GetCustomAttribute<CommandAttribute>();
            if (attr == null) continue;
            ParseMemberToNode(
                commandInstance,
                attr,
                root,
                methodInfo
            );
        }

        return (LiteralCommandNode<TCommandSender>)root;
    }

    private CommandNode<TCommandSender> ParseMemberToNode(
        object instance,
        CommandAttribute commandAttribute,
        CommandNode<TCommandSender>? node,
        MemberInfo memberInfo
    )
    {
        if (commandAttribute.Route == null)
        {
            WrapMemberToNode(instance, node!, memberInfo);
            return node!;
        }

        var reader = new CommandLineReader(commandAttribute.Route.Trim());

        return ParseMemberToNode0(node, node);

        CommandNode<TCommandSender> ParseMemberToNode0(CommandNode<TCommandSender>? currentNode,
            CommandNode<TCommandSender>? upstreamNode)
        {
            if (!reader.HasMore)
            {
                WrapMemberToNode(instance, upstreamNode!, memberInfo);
                return currentNode!;
            }

            CommandNode<TCommandSender> subNode;
            var input = reader.ReadNext()!;
            if (input.StartsWith('<'))
            {
                input = input.Trim('<', '>');
                subNode = AppendOrCreateArgumentNode(input, currentNode, memberInfo, false);
                upstreamNode = subNode;
            }
            else if (input.StartsWith('['))
            {
                input = input.Trim('[', ']');
                subNode = AppendOrCreateArgumentNode(input, currentNode, memberInfo, true);
            }
            else
            {
                subNode = AppendOrCreateLiteralNode(input, currentNode);
                upstreamNode = subNode;
            }

            return ParseMemberToNode0(subNode, upstreamNode);
        }
    }

    private ArgumentCommandNode<TCommandSender> AppendOrCreateArgumentNode(
        string key,
        CommandNode<TCommandSender>? parent,
        MemberInfo memberInfo,
        bool optional
    )
    {
        if (parent == null)
            throw new ArgumentException("An argument command node cannot be created without a parent node.");
        var methodInfo = memberInfo as MethodInfo ??
                         throw new ArgumentException(
                             $"Arguments in '{nameof(CommandAttribute)}' are only supported on methods.");

        var paramInfo = GetInfoFromParamsByKey(key, methodInfo);
        if (paramInfo == null)
            throw new ArgumentException($"No parameter named '{key}' was found in method '{methodInfo.Name}'.",
                nameof(key));
        if (!_argumentParserFactories.TryGetValue(paramInfo.ParameterType, out var factory))
            throw new ArgumentException(
                $"No argument parser factory is registered for parameter type " +
                $"'{paramInfo.ParameterType.FullName}' " +
                $"of parameter '{paramInfo.Name}' in method '{methodInfo.Name}'.");
        var node = new ArgumentCommandNode<TCommandSender>(key, factory.Create(paramInfo), optional);
        return parent.AddChild(node);
    }

    private LiteralCommandNode<TCommandSender> AppendOrCreateLiteralNode(string key,
        CommandNode<TCommandSender>? parent)
    {
        if (parent == null) return new LiteralCommandNode<TCommandSender>(key);
        var existing = parent.LiteralChildren.FirstOrDefault(x => x.Key == key);
        if (existing != null) return existing;
        var node = new LiteralCommandNode<TCommandSender>(key);
        return parent.AddChild(node);
    }

    private void WrapMemberToNode(
        object commandInstance,
        CommandNode<TCommandSender> targetNode,
        MemberInfo memberInfo
    )
    {
        if (targetNode.Handler != null)
            throw new InvalidOperationException(
                $"The command node '{targetNode.Key}' has already been wrapped by another command member."
            );

        var requirements = new List<Func<ParseContext<TCommandSender>, RequirementCheckResult>>();
        foreach (var attr in memberInfo.GetCustomAttributes())
        {
            if (!_requirementFactories.TryGetValue(attr.GetType(), out var factory)) continue;
            requirements.Add(factory.Create(attr));
        }

        if (requirements.Count > 0)
        {
            targetNode.Requirement = context =>
            {
                foreach (var requirement in requirements)
                {
                    var result = requirement(context);
                    if (result.Passed) continue;
                    return result;
                }

                return RequirementCheckResult.Pass();
            };
        }

        var methodInfo = memberInfo as MethodInfo;
        if (methodInfo == null) return;
        var parameters = methodInfo.GetParameters();
        targetNode.Handler = async c =>
        {
            var tasks = parameters.Select(async param =>
            {
                if (param.ParameterType.IsAssignableTo(typeof(TCommandSender))) return c.CommandSender;
                var key = param.GetCustomAttribute<ArgAttribute>()?.Key ?? param.Name!;
                return await c.GetArgumentAsync<object?>(key);
            });
            var args = await Task.WhenAll(tasks);
            var result = methodInfo.Invoke(commandInstance, args);
            return await HandleResultAsync(result, methodInfo.ReturnType);
        };
    }

    private static async Task<bool> HandleResultAsync(object? result, Type returnType)
    {
        if (returnType == typeof(void)) return true;
        if (returnType == typeof(bool)) return (bool)result!;
        if (returnType == typeof(Task))
        {
            await (Task)result!;
            return true;
        }

        if (returnType == typeof(Task<bool>)) return await (Task<bool>)result!;
        if (returnType == typeof(ValueTask))
        {
            await (ValueTask)result!;
            return true;
        }

        if (returnType == typeof(ValueTask<bool>)) return await (ValueTask<bool>)result!;
        throw new InvalidOperationException($"Unsupported command handler return type: {returnType}");
    }

    private ParameterInfo? GetInfoFromParamsByKey(string key, MethodInfo methodInfo)
    {
        var parameterInfo = methodInfo
            .GetParameters()
            .FirstOrDefault(param =>
                key == (param.GetCustomAttribute<ArgAttribute>()?.Key ?? param.Name)
            );
        return parameterInfo;
    }
}