
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

