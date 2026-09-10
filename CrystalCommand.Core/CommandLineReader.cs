namespace CrystalCommand.Core;

public class CommandLineReader(
    string input
)
{
    public readonly string Input = input.Trim();
    public int Pointer;
    
    public bool HasMore => Pointer < Input.Length;
    
    public char CurrentChar => Input[Pointer];
    
    public string? ReadNext()
    {
        while (HasMore && char.IsWhiteSpace(CurrentChar))
        {
            Pointer++;
        }
        if (!HasMore) return null;
        
        var start = Pointer;
        while (HasMore && !char.IsWhiteSpace(CurrentChar))
        {
            Pointer++;
        }

        return Input[start..Pointer];
    }
}