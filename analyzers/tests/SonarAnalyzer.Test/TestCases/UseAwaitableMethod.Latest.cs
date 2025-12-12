using System.IO;
using System.Threading.Tasks;
using System;

public class C
{
    public C Child { get; }
    C this[int i] => null;
    public static implicit operator int(C c) => default(C);

    C ReturnMethod() => null;
    Task<C> ReturnMethodAsync() => null;

    async Task OperatorPrecedence() // https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/operators/#operator-precedence
    {
        _ = ^ReturnMethod(); // Noncompliant
        _ = Child?.ReturnMethod()!.Child; // Noncompliant
        _ = (Child!?.Child?[0]!)?.ReturnMethod()!?.Child[0]!; // Noncompliant
        _ = (ReturnMethod()!); // Noncompliant
        _ = (ReturnMethod())!; // Noncompliant
        _ = ((ReturnMethod())!); // Noncompliant
    }

    async Task LocalFunctions()
    {
        VoidMethod(); // FN

        void VoidMethod() { }
        Task VoidMethodAsync() => null;
    }

    async Task InLocalFunction()
    {
        async Task AsyncLocalFunction(C c)
        {
            c.ReturnMethod(); // Noncompliant
        }
        void LocalFunction(C c)
        {
            c.ReturnMethod(); // Compliant
        }
    }
}

public class Sample
{
    async Task MethodInvocation(Sample sample)
    {
        sample.VoidMethod();            // Noncompliant {{Await VoidMethodAsync instead.}}
        await sample.VoidMethodAsync(); // Compliant
    }
}

public static class Extensions
{
    extension(Sample s)
    {
        public void VoidMethod() { }
        public Task VoidMethodAsync() => Task.CompletedTask;
    }
}
