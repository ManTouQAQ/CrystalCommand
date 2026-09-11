namespace CrystalCommand.Attribute.Attribute;

[AttributeUsage(AttributeTargets.Parameter)]
public class ArgAttribute(string? key = null) : System.Attribute {
    public string? Key { get; } = key;
}