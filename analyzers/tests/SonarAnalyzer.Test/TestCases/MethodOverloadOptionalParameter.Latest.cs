public record Record
{
    void Method(string[] messages) { }
    void Method(string[] messages, string delimiter = "\n") { }                 // Noncompliant
}

public partial class Partial
{
    public void Single(string[] messages) { }
    public partial void Single(string[] messages, string delimiter = "; ");     // Noncompliant

    public partial void Together(string[] messages);
    public partial void Together(string[] messages, string delimiter = "; ");   // Noncompliant

    public partial void Mixed(string[] messages);
    public partial void Mixed(string[] messages, string delimiter = "; ") { }   // Noncompliant
}

public record struct RecordStruct
{
    void Method(string[] messages) { }
    void Method(string[] messages, string delimiter = "\n") { } // Noncompliant
}


public record struct PositionalRecord(string value)
{
    void Method(string[] messages) { }
    void Method(string[] messages, string delimiter = "\n") { } // Noncompliant
}

public interface IMyInterface
{
    static abstract void Abstract(string[] messages);
    static abstract void Abstract(string[] messages, string delimiter = "\n"); // Noncompliant
//                                                   ^^^^^^^^^^^^^^^^^^^^^^^
    static virtual void Virtual(string[] messages) { }
    static virtual void Virtual(string[] messages, string delimiter = "\n") { } // Noncompliant
}

public partial class MethodOverloadOptionalParameter : IMyInterface
{
    public static void Abstract(string[] messages) { }
    public static void Abstract(string[] messages, string delimiter = "\n") { } // Compliant; comes from interface
    public static void Virtual(string[] messages) { }
    public static void Virtual(string[] messages, string delimiter = "\n") { } // Compliant; comes from interface

    partial void Print(string[] messages);

    partial void Print(string[] messages) { }

    void Print(string[] messages, string delimiter = "\n") { } // Noncompliant;
    void Print(string[] messages,
        string delimiter = "\n", // Noncompliant
        string b = "a" // Noncompliant
        )
    { }
}

public class MyClass1(string[] messages)
{
    public MyClass1(string[] messages, string delimiter = "\n") : this(messages) // Noncompliant {{This method signature overlaps the one defined on line 60, the default parameter value can't be used.}}
    {
    }
}

public class MyClass2(string[] messages, string delimiter = "\n") // Noncompliant {{This method signature overlaps the one defined on line 69, the default parameter value can't be used.}}
{
    public MyClass2(string[] messages) : this(messages, "\n")
    {
    }
}

public partial class PartialConstructor
{
    public partial PartialConstructor(string[] messages) { }
    public partial PartialConstructor(string[] messages, string delimiter = "\n");
}

public class Sample { }

public class SampleWithMethod
{
    public void Method(string[] messages) { }
}

public static class NewExtensions
{
    extension(Sample sample)
    {
        public void Method(string[] messages) { }
        public void Method(string[] messages, string delimiter = "\n") { } // Noncompliant
    }

    extension(SampleWithMethod sample)
    {
        public void Method(string[] messages, string delimiter = "\n") { }
    }
}
