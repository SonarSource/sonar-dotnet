using System;

int TopLevelLocalfunction() => 0;       // FN

var localVariable = 42;
int LocalFunction() => localVariable;   // Compliant

public record Methods
{
    private int instanceMember;
    private static int staticMember;

    public int Method1() => 0;                              // Noncompliant {{Make 'Method1' a static method.}}
    public int Method2() => instanceMember;
    public int Method3() => this.instanceMember;
    public int Method4() => staticMember;                   // Noncompliant
    public int Method5() => Methods.staticMember;           // Noncompliant
    public int Method6() => new Methods().instanceMember;   // Noncompliant
    public int Method7(Methods arg) => arg.instanceMember;  // Noncompliant

    public static int StaticMethod1() => 0;
    public static int StaticMethod2() => staticMember;
}

public class Lambda
{
    private int instanceMember;
    private static int staticMember;

    public void Method()
    {
        var variable = 0;
        Execute(() => instanceMember);
        Execute(() => variable);

        // These lambdas could be static, but the rule doesn't apply to lambdas
        Execute(() => 0);
        Execute(() => staticMember);
    }

    private void Execute(Func<int> f) { }
}
