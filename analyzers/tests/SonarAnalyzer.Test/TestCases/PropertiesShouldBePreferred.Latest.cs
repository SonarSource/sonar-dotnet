using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

public record RecordBase
{
    public virtual string GetFooMethod() => ""; // Noncompliant
}

public record Record : RecordBase
{
    private string name;

    public string GetName() // Noncompliant
    {
        return name;
    }

    public override string GetFooMethod() => ""; // Compliant

    public object GetName2() => null; // Noncompliant

    protected string GetName3() => null; // Noncompliant

    protected internal string GetName4() => null; // Noncompliant

    private string GetName5() => null; // Compliant

    public string Property { get; set; }
}

public record PositionalRecord(string Parameter) : RecordBase
{
    private string name;

    public string GetName() // Noncompliant
    {
        return name;
    }

    public override string GetFooMethod() => ""; // Compliant

    public object GetName2() => null; // Noncompliant

    protected string GetName3() => null; // Noncompliant

    protected internal string GetName4() => null; // Noncompliant

    private string GetName5() => null; // Compliant

    public string Property { get; set; }
}

namespace MyLibrary
{
    public interface IBase
    {
        static abstract string GetStuff(); // Noncompliant
    }

    public class SomeClass : IBase
    {
        public static string GetStuff() => "";
    }
}

public record struct RecordStruct
{
    public RecordStruct() { }

    private string name = "";

    public string GetName() // Noncompliant {{Consider making method 'GetName' a property.}}
    //            ^^^^^^^
    {
        return name;
    }

    public object GetName2() => null;  // Noncompliant
    private string GetName3() => null; // Compliant
    public string Property { get; set; } = "";
}

public record struct PositionalRecordStruct(string Parameter)
{
    private string name = "";

    public string GetName()            // Noncompliant
    {
        return name;
    }

    public object GetName2() => null;  // Noncompliant
    private string GetName3() => null; // Compliant
    public string Property { get; set; } = "";
}

public class Sample { }

[AttributeUsage(AttributeTargets.Method)]
public sealed class MethodOnlyAttribute : Attribute { }

public static class Extensions
{
    extension(Sample sample)
    {
        public int ExtensionProp => 1;
        public int GetExtensionMethod() => 1;   // Noncompliant

        private int GetValue() => 1;
        public Task<int> GetTaskValue() => Task.FromResult(1);
        public int[] GetArrayValue() => null;

        [MethodOnly]
        public int GetAttributedMethod() => 1;
    }


    extension(Sample)
    {
        public static int StaticExtensionProp => 1;
        public static int GetStaticExtensionMethod() => 1;  // Noncompliant
        public static int StaticExtensionGetSetProp
        {
            get => 1;
            set { }
        }
    }

    extension<T>(IEnumerable<T> source)
    {
        public int GetCount() => 1;         // Noncompliant - non-generic method in a generic extension block
        public U GetValue<U>() => default;
    }
}
