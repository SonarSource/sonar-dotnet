using System;
using System.Threading;
using System.Threading.Tasks;

public interface ISomeInterface
{
    static abstract Task<string> StaticVirtualMembersInInterfaces();
}

public class SomeClass : ISomeInterface
{
    public static Task<string> StaticVirtualMembersInInterfaces()
    {
        return Task.Run(() => "Test");
    }
}

public class TestClass
{
    void Method()
    {
        var x = SomeClass.StaticVirtualMembersInInterfaces().Result; // Noncompliant {{Replace this use of 'Task.Result' with 'await'.}}
        SomeClass.StaticVirtualMembersInInterfaces().GetAwaiter().GetResult(); // Noncompliant

        Task.Run(SomeClass.StaticVirtualMembersInInterfaces).GetAwaiter().GetResult(); // Compliant
    }
}

