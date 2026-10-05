using System;
using System.Collections.Generic;
using System.IO;

class LocalVariableDeclarationAssignmentShouldBeJoined
{
    private int x;

    LocalVariableDeclarationAssignmentShouldBeJoined(int value)
    {
        int y;      // Noncompliant
        y = value;
    }

    void BasicTypes()
    {
        int x;      // Noncompliant {{Join the declaration and the assignment of this local variable.}}
//          ^
        x = 42;

        string s;   // Noncompliant
        s = "hello";

        object o;   // Noncompliant
        o = new object();

        List<int> list; // Noncompliant
        list = new List<int>();
    }

    void MethodCallAssignment()
    {
        int result; // Noncompliant
        result = ComputeValue();
    }

    void AlreadyInitialized()
    {
        int x = 0;          // Compliant
        string s = "hello"; // Compliant
        var v = 42;         // Compliant
    }

    void InterveningStatement()
    {
        int x;              // Compliant - intervening statement
        DoSomething();
        x = 42;
    }

    void ConditionalNext()
    {
        int x;              // Compliant - next statement is conditional
        if (true)
            x = 1;
        else
            x = 2;
    }

    void LoopNext()
    {
        int x;              // Compliant - next statement is a loop
        while (true)
        {
            x = 1;
            break;
        }
    }

    void TryCatchNext()
    {
        int x;              // Compliant - next statement is try-catch
        try
        {
            x = int.Parse("1");
        }
        catch
        {
            x = 0;
        }
    }

    void CompoundAssignment()
    {
        int x;              // Compliant - compound assignment
        x += 1;             // Error [CS0165]
    }

    void MultipleDeclarators()
    {
        int x, y;           // Compliant - multiple declarators
        x = 1;
    }

    void FieldAssignment()
    {
        int x;              // Compliant - assigned to field, not to local
        this.x = 1;
    }

    void AssignmentToOtherVariable()
    {
        int x;              // Compliant - intervening declaration
        int y = 0;
        x = 1;
    }

    void UsingStatement()
    {
        using (var stream = File.OpenRead("test")) { } // Compliant - using statement
    }

    void ParameterAssignment(int value)
    {
        int x;              // Noncompliant
        x = value;
    }

    int InPropertyGetter
    {
        get
        {
            int x;          // Noncompliant
            x = 42;
            return x;
        }
    }

    void InLambda()
    {
        Action action = () =>
        {
            int x;          // Noncompliant
            x = 42;
        };
    }

    void InLocalFunction()
    {
        void Local()
        {
            int x;          // Noncompliant
            x = 42;
        }
    }

    void ForLoopVariable()
    {
        for (int i = 0; i < 10; i++) { } // Compliant - for loop variable
    }

    void ForeachVariable()
    {
        foreach (var item in new[] { 1 }) { } // Compliant - foreach variable
    }

    void NullableType()
    {
        int? x;             // Noncompliant
        x = null;
    }

    void DeclarationFollowedByNonAssignment()
    {
        int x;              // Compliant - next statement is not an assignment
        TryGet(out x);
        Console.WriteLine(x);
    }

    void DeclarationAtEndOfBlock()
    {
        int x;              // Compliant - no following statement
    }

    void TernaryAssignment()
    {
        int x;              // Noncompliant
        x = true ? 1 : 2;
    }

    void NullCoalescingAssignment()
    {
        string s;           // Noncompliant
        s = null ?? "default";
    }

    async System.Threading.Tasks.Task AwaitAssignment()
    {
        System.Threading.Tasks.Task<int> task = System.Threading.Tasks.Task.FromResult(1);
        int x;              // Noncompliant
        x = await task;
    }

    void SelfReferencingLambda()
    {
        Func<int, int> fact = null; // Compliant - already initialized
        fact = n => n <= 1 ? 1 : n * fact(n - 1);
    }

    void SelfUnsubscribingEventHandler()
    {
        EventHandler h = null;      // Compliant - already initialized
        h = (s, e) => { Changed -= h; };
    }

    void SelfReferenceInRightHandSide()
    {
        int x;                      // Compliant - right-hand side references the declared variable
        x = TryGet(out x) ? x : 0;
    }

    void SelfReferenceWriteOnly()
    {
        Action a;                   // Compliant - FN: a pure write of the declared variable in the right-hand side would compile once joined
        a = () => a = null;
    }

    void LambdaCapture()
    {
        int x;                      // Compliant - next statement is a declaration
        Action a = () => x = 123;
        a();
    }

    void CrossVariableWrite()
    {
        Action a;                   // Compliant - next statement is a declaration
        int x;                      // Compliant - assignment targets another variable
        a = () => x = 42;
    }

    void Deconstruction()
    {
        int x;                      // Noncompliant
        (x, _) = (1, 2);
        int y;                      // Noncompliant
        (_, y) = (1, 2);
    }

    unsafe void PointerType(int value)
    {
        int* p;                     // Compliant - pointer type
        p = &value;
    }

    event EventHandler Changed;

    void MultipleViolations()
    {
        int a;              // Noncompliant
        a = 1;
        int b;              // Noncompliant
        b = 2;
    }

    void MixedCompliantAndNoncompliant()
    {
        int a = 0;          // Compliant
        int b;              // Noncompliant
        b = 1;
        int c;              // Compliant - intervening statement
        Console.WriteLine(b);
        c = 2;
    }

    void SwitchNext(int value)
    {
        int x;              // Compliant - next statement is switch
        switch (value)
        {
            case 0:
                x = 1;
                break;
            default:
                x = 2;
                break;
        }
    }

    private int ComputeValue() => 42;
    private void DoSomething() { }
    private bool TryGet(out int value) { value = 1; return true; }
}
