namespace CrystalCommand.Attribute.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false)]
public class CommandAttribute(string? route = null) : System.Attribute {
    public string? Route { get; } = route;
}