namespace Tests.Diagnostics
{
    public class MyClass1(string[] messages)
    {
        public MyClass1(string[] messages, string delimiter = "\n") : this(messages) // Noncompliant {{This method signature overlaps the one defined on line 3, the default parameter value can't be used.}}
        {
        }
    }

    public class MyClass2(string[] messages, string delimiter = "\n") // Noncompliant {{This method signature overlaps the one defined on line 12, the default parameter value can't be used.}}
    {
        public MyClass2(string[] messages) : this(messages, "\n")
        {
        }
    }
}
