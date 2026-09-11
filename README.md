# CrystalCommand

只是一个 .Net 命令框架

CrystalCommand 提供基于 Attribute 的驱动方式，同时支持：

* 命令树
* 强类型参数
* 可选参数
* 异步命令处理
* 自定义参数解析器
* 自定义 Requirement

## 安装

`CrystalCommand.Core` 提供命令树、解析和执行等基础功能。
```bash
dotnet add package CrystalCommand.Core
```
`CrystalCommand.Attribute` 基于 Core, 提供了命令被 Attribute 驱动的能力。
```bash
dotnet add package CrystalCommand.Attribute
```

## 快速开始

使用 Attribute 定义命令：

```csharp
using CrystalCommand.Attribute.Arguments;
using CrystalCommand.Attribute.Attribute;

[Command("test")]
public class TestCommand
{
    // test
    [Command]
    public void Root(TestSender sender)
    {
        sender.Result = "test";
    }
    
    // test hello
    [Command("hello")]
    public void Hello(TestSender sender)
    {
        sender.Result = "hello";
    }
    
    // test params qwe
    // test params qwe 233
    [Command("params <p1> [p2]")]
    public void Params(TestSender sender, string p1, [Max(10)] [Clamp] int p2)
    {
        sender.Result = $"test params {p1} {p2}";
    }
    
    // test send "qwe qwe qwe"
    [Command("send <msg>")]
    public void SendMsg(TestSender sender, [Quoted] string msg)
    {
        sender.Result = $"test send {msg}";
    }
    
    [Command("admin <name>")]
    [Permission("admin.add")]
    public void AddAdmin(TestSender sender, string name)
    {
        sender.Result = $"test admin {name}";
    }
    
    [Command("admin settings [k] [v]")]
    [Permission("admin.settings")]
    public void AdminSettings(TestSender sender, [Arg("k")] string? key, [Arg("v")] string? value)
    {
        sender.Result = $"test admin settings { key ?? "null"} { value ?? "null"}";
    }
}
```

上面的定义会生成类似这样的命令树：

```text
test
├── hello
├── params <p1> [p2]
├── send <msg>
└── admin
    ├── <name>
    └── settings [k] [v]
```

使用 `AttributeCommandParser` 来解析并注册它

```csharp
    CommandManager<TestSender> commandManager = new();
    var parser = new AttributeCommandParser<TestSender>("tester", commandManager); // 这个 "tester" 是命名空间
    parser.RegisterRequirement(new PermissionRequirementFactory()); // 这里是自定义 Requirement
    parser.ParseCommand(new TestCommand());
    
    await commandManager.ExecuteAsync(new TestSender(), "foo bar baz"); // 执行命令
```

## 自定义参数解析器

下面是 CrystalCommand 内置的对于 string 参数的解析:

```csharp
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
```

使用 `AttributeCommandParser#RegisterArgumentParser`方法来注册到CommandParser当中。

## 自定义 Requirement

上面例子中的 `[Permission]` 等 Requirement 的定义是通过Factory来创建的.

```csharp
public class PermissionRequirementFactory : IRequirementFactory<TestSender, PermissionAttribute>
{
    public Func<ParseContext<TestSender>, RequirementCheckResult> Create(PermissionAttribute attribute)
    {
        return c => c.CommandSender.HasPermission(attribute.Permission)
            ? RequirementCheckResult.Pass()
            : RequirementCheckResult.Failure($"do not have permission: {attribute.Permission}");
    }
}
```

将这个Factory通过 `AttributeCommandParser#RegisterRequirement` 方法注册后就可以直接使用。