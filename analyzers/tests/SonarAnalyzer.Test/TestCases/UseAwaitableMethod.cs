using System;
using System.IO;
using System.Threading.Tasks;

public class C
{
    public C Child { get; }
    public Action ActionProperty { get; }
    public Func<Task> ActionPropertyAsync { get; }

    void VoidMethod() { }
    Task VoidMethodAsync() => Task.CompletedTask;

    C ReturnMethod() => null;
    Task<C> ReturnMethodAsync() => Task.FromResult<C>(null);

    bool BoolMethod() => true;
    Task<bool> BoolMethodAsync() => Task.FromResult(true);

    T GenericMethod<T>() => default(T);
    Task<T> GenericMethodAsync<T>() => Task.FromResult(default(T));

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
        VoidMethod(); // Noncompliant {{Await VoidMethodAsync instead.}}
        await VoidMethodAsync(); // Compliant
        VoidMethodAsync(); // Compliant: https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/compiler-messages/cs4014
        this.VoidMethod(); // Noncompliant
        this.Child?.VoidMethod(); // Noncompliant
        this.Child.Child?.VoidMethod(); // Noncompliant

        ReturnMethod(); // Noncompliant
        _ = ReturnMethod(); // Noncompliant
        this.ReturnMethod().ReturnMethod().ReturnMethod();
//      ^^^^^^^^^^^^^^^^^^^                                  
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^                   @-1
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^    @-2
        _ = true ? ReturnMethod() : ReturnMethod();
        //         ^^^^^^^^^^^^^^
        //                          ^^^^^^^^^^^^^^           @-1
        await ReturnMethod().ReturnMethodAsync(); // Noncompliant
        //    ^^^^^^^^^^^^^^

        GenericMethod<bool>(); // Noncompliant

        // Delegate invokes. Do not report and crash
        Func<Action> f = new Func<Action>(() => () => { });
        f()(); // Compliant
        ActionProperty(); // Compliant
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
        _ = (ReturnMethod()); // Noncompliant
        _ = nameof(VoidMethod); // Compliant
        _ = !BoolMethod(); // Noncompliant
        _ = BoolMethod() ? ReturnMethod() : default(C);
        //  ^^^^^^^^^^^^
        //                 ^^^^^^^^^^^^^^ @-1
        _ = +ReturnMethod(); // Noncompliant
        _ = -ReturnMethod(); // Noncompliant
        _ = !ReturnMethod(); // Noncompliant
        _ = ~ReturnMethod(); // Noncompliant
        _ = ReturnMethod() + default(C); // Noncompliant
        _ = ReturnMethod() - default(C); // Noncompliant
        _ = ReturnMethod() - !ReturnMethod();
        //  ^^^^^^^^^^^^^^
        //                    ^^^^^^^^^^^^^^ @-1
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
    public Task<int> ImplicitConversionsMethodAsync(long otherName1, int otherName2) => Task.FromResult(0);
    public Task<byte> ImplicitConversionsMethodAsync(byte otherName1, byte otherName2) => Task.FromResult((byte)0);

    public void TypeParameter(C c) { }
    public Task TypeParameter<T>(T t) where T : C => Task.CompletedTask;

    public async Task Test(int i, byte j)
    {
        long l1 = ImplicitConversionsMethod(i, j);              // Noncompliant Can be resolved to first overload
        long l2 = ImplicitConversionsMethod(i, (IComparable)j); // Compliant No fitting overload
        var l3 = ImplicitConversionsMethod((byte)i, j);         // Noncompliant Can be resolved to second overload
        int l4 = (int)ImplicitConversionsMethod((byte)i, j);    // Noncompliant Can be resolved to second overload

        TypeParameter(new C()); // Compliant: Adding "await" does never resolve to another overload
    }
}

public class Inheritance
{
    class Child : Inheritance
    {
        public void OuterVoidMethod() { }
        public void InnerVoidMethod() { }
    }

    class GrandChild : Child
    {
        public GrandChild Chain { get; }

        public async Task Test()
        {
            OuterVoidMethod();                    // Noncompliant
            this.OuterVoidMethod();               // Noncompliant
            (this as Child).OuterVoidMethod();    // Noncompliant
            this.Chain?.OuterVoidMethod();        // Noncompliant
            this.Chain?.Chain?.OuterVoidMethod(); // Noncompliant
            InnerVoidMethod();                    // Noncompliant
            this.InnerVoidMethod();               // Noncompliant
            (this as Child).InnerVoidMethod();    // Compliant: InnerVoidMethodAsync is defined on GrandChild
            this.Chain?.InnerVoidMethod();        // Noncompliant
            this.Chain?.Chain?.InnerVoidMethod(); // Noncompliant
        }

        public Task InnerVoidMethodAsync() => Task.CompletedTask;
    }

    public Task OuterVoidMethodAsync() => Task.CompletedTask;

    async Task Test()
    {
        var grandChild = new GrandChild();
        grandChild.OuterVoidMethod();         // Noncompliant
        grandChild?.OuterVoidMethod();        // Noncompliant
        grandChild?.Chain?.OuterVoidMethod(); // Noncompliant
        grandChild.InnerVoidMethod();         // Noncompliant
        grandChild?.InnerVoidMethod();        // Noncompliant
        grandChild?.Chain?.InnerVoidMethod(); // Noncompliant
    }
}

class AsynchronousLambdas
{
    async Task CallAsyncLambda(StreamReader reader)
    {
        await Task.Run(async () =>
        {
            await Foo();
            reader.ReadLine(); // Noncompliant
        });
        Func<Task> a = async () =>
        {
            await Foo();
            reader.ReadLine(); // Noncompliant  
        };
        Func<Task> b = async delegate ()
        {
            await Foo();
            reader.ReadLine(); // Noncompliant  
        };
    }

    async Task<Action> CreateActionAsync(StreamReader reader)
    {
        Action action = () =>
        {
            reader.ReadLine();      // Compliant
        };
        return action;
    }

    Task Foo() => null;
}
