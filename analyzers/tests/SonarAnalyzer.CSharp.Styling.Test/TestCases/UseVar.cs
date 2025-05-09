using System.Collections.Generic;

public class UseVar
{
    public static Dictionary<string, string> field = new();
    public static List<string> Property { get; set; } = new();

    List<string> FromMethod() => new();

    List<string> Method(bool condition, List<string> optional = new()) // Error [CS1736]
    {
        field = new();
        Property = new();

        int uninitializedValueType;
        List<string> uninitialized;

        var list = new List<string>();
        var array = new string[0];
        List<string> multiple1 = new(), multiple2 = new();
        List<string> ternary = condition ? new() : new();
        IEnumerable<string> canNotInfer = condition ? list : array;

        var invalidMultiple1 = new List<string>(), invalidMultiple2 = new List<string>();   // Error [CS0819]
        var nottyped = new();                                                               // Error [CS8754]

        List<string> noncompliant = new();                                  // Noncompliant {{Use var.}}
//      ^^^^^^^^^^^^
        List<string> fromInvocation = FromMethod();                         // Noncompliant
        List<string> listRenamed = list;                                    // Noncompliant
        List<string> propertyRenamed = UseVar.Property;                     // Noncompliant
        Dictionary<string, string> filedRenamed = global::UseVar.field;     // Noncompliant

        List<string> redundant = new List<string>();                        // IDE0007

        return new();
    }
}
