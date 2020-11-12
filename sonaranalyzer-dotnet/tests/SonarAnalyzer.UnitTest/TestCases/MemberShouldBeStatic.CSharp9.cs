
int TopLevelMethod() => 0;              // FN

var localVariable = 42;
int LocalFunction() => localVariable;   // Compliant

public record Methods
{
    private int instanceMember = 0;
    private static int staticMember = 0;

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

