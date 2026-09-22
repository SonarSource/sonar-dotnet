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
        _ = Child?.ReturnMethod()![0]; // Noncompliant
        _ = (Child?.ReturnMethod())![0]; // Noncompliant
        _ = Child?.ReturnMethod()[0]!; // Noncompliant
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
    public static System.Runtime.CompilerServices.TaskAwaiter GetAwaiter(this Task[] tasks) => Task.WhenAll(tasks).GetAwaiter();

    extension(Sample s)
    {
        public void VoidMethod() { }
        public Task VoidMethodAsync() => Task.CompletedTask;
    }
}

public class Node
{
    public Node Child() => this;
    public Task<Node> ChildAsync() => Task.FromResult(this);
    public Node this[string key] => this;
    public Task[] Tasks() => [Task.CompletedTask];
    public Task<Task[]> TasksAsync() => Task.FromResult(Tasks());
    public Node[] Array() => [this];
    public Task<Node[]> ArrayAsync() => Task.FromResult(Array());
}

public class Repro
{
    public async Task<Node?> Crash(Node? node)
    {
        await Task.Yield();
        return node?.Child()["k"]; // Noncompliant {{Await ChildAsync instead.}}
    }

    public async Task IndexedInvocations(Node? node)
    {
        _ = node?.Array()[1..];    // Noncompliant {{Await ArrayAsync instead.}}
        await node?.Tasks()![0];   // Noncompliant {{Await TasksAsync instead.}}
        await (node?.Tasks())[0]!; // Noncompliant
        await (node?.Tasks()[0])!; // Noncompliant
        await node!.Tasks()!;      // Compliant: Awaiting the array via the GetAwaiter extension method.
        await (node?.Tasks())!;    // Compliant
        await (node?.Tasks()!);    // Compliant
    }

    public async Task ChainedInvocations(Node? node)
    {
        _ = node?.Child()?.Child();          // Noncompliant [conditionalFirst, conditionalSecond]
        _ = node?.Child()!.Child();          // Noncompliant [conditionalForgivingFirst, conditionalForgivingSecond]
        _ = node!.Child()?.Child();          // Noncompliant [forgivingConditionalFirst, forgivingConditionalSecond]
        _ = node!.Child()!.Child();          // Noncompliant [forgivingFirst, forgivingSecond]
        _ = node?.Child()!.Child()?["k"];    // Noncompliant [conditionalIndexFirst, conditionalIndexSecond]
        _ = node?.Child()?.Child()!["k"];    // Noncompliant [forgivingIndexFirst, forgivingIndexSecond]
        _ = node?.Child()!.Child()!.Child(); // Noncompliant [nestedFirst, nestedSecond, nestedThird]
        _ = (node?.Child())!.Child();        // Noncompliant [parenthesizedFirst, parenthesizedSecond]
    }
}

public class DelegateNode
{
    public DelegateNode this[int key] => this;

    public DelegateNode Child() => this;
    public Task<DelegateNode> ChildAsync() => Task.FromResult(this);
    public Func<DelegateNode> Factory() => () => this;
    public Task<Func<DelegateNode>> FactoryAsync() => Task.FromResult(Factory());
    public Func<Func<DelegateNode>> NestedFactory() => () => Factory();
    public Task<Func<Func<DelegateNode>>> NestedFactoryAsync() => Task.FromResult(NestedFactory());

    public async Task DelegateInvocations(DelegateNode node)
    {
        _ = node?.Factory()();         // Noncompliant {{Await FactoryAsync instead.}}
        _ = node?.Factory()()[0];      // Noncompliant
        _ = node?.Factory()!();        // Noncompliant
        _ = node?[0].Factory()();      // Noncompliant
        _ = node?.NestedFactory()()(); // Noncompliant {{Await NestedFactoryAsync instead.}}
        _ = node?.Factory().Invoke();  // Noncompliant
        _ = node?.Factory()?.Invoke(); // Noncompliant
        _ = node?.Factory()!.Invoke(); // Noncompliant
        _ = (node?.Factory())();       // Noncompliant
        _ = node.Factory()();          // Noncompliant
    }

    public async Task ChainedDelegateInvocations(DelegateNode node)
    {
        _ = node?.Child()!.Factory()(); // Noncompliant [conditionalForgivingChild, conditionalForgivingFactory]
        _ = node?.Child()?.Factory()(); // Noncompliant [conditionalChild, conditionalFactory]
        _ = node!.Child()!.Factory()(); // Noncompliant [forgivingChild, forgivingFactory]
        _ = node?.Factory()()?.Child(); // Noncompliant [conditionalResultFactory, conditionalResultChild]
        _ = node?.Factory()()!.Child(); // Noncompliant [forgivingResultFactory, forgivingResultChild]
    }
}
