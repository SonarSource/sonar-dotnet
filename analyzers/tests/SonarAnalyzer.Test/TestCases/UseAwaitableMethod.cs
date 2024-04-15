using System.IO;
using System.Threading.Tasks;
using System;

public class C
{
    public C Child { get; }
    void VoidMethod() { }
    Task VoidMethodAsync() => Task.CompletedTask;

    C ReturnMethod() => null;
    Task<C> ReturnMethodAsync() => Task.FromResult<C>(null);

    bool BoolMethod() => true;
    Task<bool> BoolMethodAsync() => Task.FromResult(true);

    C this[int i] => null;
    public static C operator +(C c) => default(C);
    public static C operator +(C c1, C c2) => default(C);
    public static C operator -(C c) => default(C);
    public static C operator -(C c1, C c2) => default(C);
    public static C operator !(C c) => default(C);
    public static C operator ~(C c) => default(C);
    public static implicit operator int(C c) => default(C);

    async Task MethodInvocations()
    {
        VoidMethod(); // Noncompliant
        await VoidMethodAsync(); // Compliant
        VoidMethodAsync(); // Compliant: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs4014
        this.VoidMethod(); // Noncompliant
        this.Child?.VoidMethod(); // Noncompliant
        this.Child.Child?.VoidMethod(); // Noncompliant

        ReturnMethod(); // Noncompliant
        _ = ReturnMethod(); // Noncompliant
        this.ReturnMethod().ReturnMethod().ReturnMethod();
        //   ^^^^^^^^^^^^                                  
        //                  ^^^^^^^^^^^^                   @-1
        //                                 ^^^^^^^^^^^^    @-2
        _ = true ? ReturnMethod() : ReturnMethod();
        //         ^^^^^^^^^^^^
        //                          ^^^^^^^^^^^^           @-1
    }

    public void NonAsyncMethod_VoidReturning()
    {
        VoidMethod(); // Compliant
    }

    public Task NonAsyncMethod_TaskReturning()
    {
        VoidMethod(); // Compliant: Enclosing Method must be async
        return Task.CompletedTask;
    }

    async Task OperatorPrecedence() // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/#operator-precedence
    {
        _ = new C().ReturnMethod(); // Noncompliant
        this[0].VoidMethod(); // Noncompliant
        Child[0].VoidMethod(); // Noncompliant
        Child.Child[0]?.VoidMethod(); // Noncompliant
        Child?.Child[0].VoidMethod(); // Noncompliant
        Child?.Child?[0].VoidMethod(); // Noncompliant
        Child.Child?[0].VoidMethod(); // Noncompliant
        Child?.Child?[0]?.VoidMethod(); // Noncompliant
        _ = Child?.Child?[0]?.ReturnMethod()?.Child[0]; // Noncompliant
        _ = nameof(VoidMethod); // Compliant
        _ = !BoolMethod(); // Noncompliant
        _ = BoolMethod() ? ReturnMethod() : default(C);
        //  ^^^^^^^^^^
        //                 ^^^^^^^^^^^^
        _ = +ReturnMethod(); // Noncompliant
        _ = -ReturnMethod(); // Noncompliant
        _ = !ReturnMethod(); // Noncompliant
        _ = ReturnMethod() + default(C); // Noncompliant
        _ = ReturnMethod() - default(C); // Noncompliant
        _ = ReturnMethod() - !ReturnMethod();
        //  ^^^^^^^^^^^^
        //                    ^^^^^^^^^^^^ @-1
    }

    async Task ExtensionMethods()
    {
        this.ExtVoidMethod(); // Noncompliant
    }
}

public static class Extensions
{
    public static void ExtVoidMethod(this C c) { }
    public static Task ExtVoidMethodAsync(this C c) => Task.CompletedTask;
}

public class Overloads
{
    public long ImplicitConversionsMethod(int i, IComparable j) => 0;
    public Task<int> ImplicitConversionsMethodAsync(long otherName1, long otherName2) => Task.FromResult(0);
    public Task<byte> ImplicitConversionsMethodAsync(byte otherName1, byte otherName2) => Task.FromResult((byte)0);

    public void TypeParameter(C c) { }
    public Task TypeParameter<T>(T t) where T : C => Task.CompletedTask;

    public async Task Test(int i, IComparable j)
    {
        long l1 = ImplicitConversionsMethod(i, j); // Noncompliant (Can be resolved to first overload)
        TypeParameter(new C()); // Compliant: Adding "await" does never resolve to another overload
    }
}

public class Inheritance
{
    class Child : Inheritance
    {
        public void VoidMethod() { }
    }

    class GrandChild : Child
    {
        public async Task Test()
        {
            VoidMethod(); // Noncompliant
        }
    }

    public Task VoidMethodAsync() => Task.CompletedTask;
}

class AsynchronousLambdas
{
    void CallAsyncLambda(string path)
    {
        Task.Run(async () => {
            await Foo();
            File.ReadAllLines(path); // Noncompliant
        });
        Func<Task> a = async () =>
        {
            await Foo();
            File.ReadAllLines(path); // Noncompliant  
        };
        Func<Task> b = async delegate ()
        {
            await Foo();
            File.ReadAllLines(path); // Noncompliant
        };
    }

    Task Foo() => null;
}
