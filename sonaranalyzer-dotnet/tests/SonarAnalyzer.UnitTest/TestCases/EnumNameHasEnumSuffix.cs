namespace Tests.Diagnostics
{
    public enum MyEnum //Noncompliant {{Rename this enumeration to remove the 'Enum' suffix.}}
//              ^^^^^^
    {
        Value
    }
    public enum MyFlags //Noncompliant {{Rename this enumeration to remove the 'Flags' suffix.}}
    {
        Value
    }
    public enum MyEnum2
    {
        Value
    }
    public class ClassEnum
    {
    }
}
