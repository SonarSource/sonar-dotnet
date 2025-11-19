
public interface IVirtualMethods
{
    virtual static string StaticVirtualMembersInInterfaces(string s1, string s2) => s1 + s2;
    
    virtual static string Prop { get; set; }
}

public class VirtualMethods : IVirtualMethods // Compliant, classes that implement interfaces cannot be static
{

}

public interface IAbstractMethods
{
    static abstract bool StaticVirtualMembersInInterfaces();
}

public class AbstractMethods : IAbstractMethods // Compliant, classes that implement interfaces cannot be static
{
    public static bool StaticVirtualMembersInInterfaces() => true;
}

public class PrimaryCtorClass() // Noncompliant {{Remove this primary constructor.}}
{
    public static string Concatenate(string s1, string s2)
    {
        return s1 + s2;
    }

    public static string Prop { get; set; }
}

public class PrimaryCtorClassWithParams(int i) // Compliant There is a captured parameter for the instance 
{
    public static string Concatenate(string s1, string s2)
    {
        return s1 + s2;
    }
}

namespace CSharp13
{
    public partial class PartialStaticProperty // Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        public static partial string Prop { get; set; }
    }
}

namespace CSharp14
{
    public static class TestClass
    {
        extension(TestClass)
        {
            public static string Prop => "42";
        }
    }

    public class StaticFieldKeyword // Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        public static int Prop { get => field++; set => field++; }
    }

    public class FieldKeyword
    {
        public int Prop { get => field++; set => field++; }
    }

    public partial class StaticPartialConstructor // Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        static partial StaticPartialConstructor();    // Error [CS0267]
    }

    public partial class PartialConstructor
    {
        partial PartialConstructor();
    }

    public partial class StaticPartialEvent // Noncompliant {{Add a 'protected' constructor or the 'static' keyword to the class declaration.}}
    {
        public static partial event System.EventHandler<System.EventArgs> PartialEvent;
    }

    public partial class InstancePartialEvent
    {
        public partial event System.EventHandler<System.EventArgs> PartialEvent;
    }
}
