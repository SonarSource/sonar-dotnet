using System;
using System.Threading.Tasks;

decimal dec = 3 / 2;    // Noncompliant
dec = 3L / 2;           // Noncompliant
Method(3 / 2);          // Noncompliant
dec = (decimal)3 / 2;
Method(3.0F / 2);

nint a = 3;
nint b = 2;
nuint c = 5;
decimal both = a / b;           // Noncompliant
decimal left = (nint)3 / 2;     // Noncompliant
decimal right = 3 / (nint)2;    // Noncompliant
left = c / 2;                   // Noncompliant
left = (nuint)3 / 2;            // Noncompliant
Method((nint)3 / 2);            // Noncompliant

right = (decimal)3 / b;         // Compliant
Method(3.0F / b);               // Compliant

left = (UnknownType)3 / 2;      // Error [CS0246]

void Method(float f) { }

static double Calc()
{
    return (nint)3 / 2;     // Noncompliant
}

public class Sample
{
    public decimal Property { get; set; }

    public void M()
    {
        (Property, var _) = (3 / 2, 3 / 2);    // Noncompliant {{Cast one of the operands of this division to 'decimal'.}}
//                           ^^^^^

        (decimal x, var y) = (1 / 3, 2);      // Noncompliant
        (decimal xx, var (yy, zz)) = (1 / 3, (2, 1 / 3)); // Noncompliant
        (decimal xxx, var (yyy, zzz)) = (1, (2, 1 / 3));
        (decimal xxxx, var (yyyy, zzzz)) = (1, (2, 1 / 3)); // FN


        var (a, b) = (1 / 3, 2);
        var (a1, (b1, c1)) = (1, (1 / 3, 2));
        var d = (1 / 3, 2);
        (int, int) d2 = (1 / 3, 2);
        (decimal, decimal) d3 = (1 / 3, 2); // Noncompliant
        (int, (decimal, (int, decimal))) nested = (3, (1 / 3, (0, 1 / 3)));
        //                                             ^^^^^ {{Cast one of the operands of this division to 'decimal'.}}
        //                                                        ^^^^^ @-1 {{Cast one of the operands of this division to 'decimal'.}}

        (decimal, decimal) d4 = (1 / 3, 2); // Noncompliant
        (decimal, decimal) d5 = (1 / 3, 1 / 3); // Noncompliant [issue3, issue4]
        (decimal d6, decimal d7) = (1 / 3, 1 / 3); // Noncompliant [issue5, issue6]

        var bar = (1, FooDecimal(1 / 3)); // Noncompliant

        (int, decimal) fooBar;
        fooBar = (1, FooDecimal(1 / 3)); // Noncompliant

        var p1 = 1;
        var p2 = 2;
        (int, decimal) result = (1, p1 / p2); // Noncompliant

        (int, ValueTuple<int, int>) vt = (1, (1 / 3, 3)); // FN
        (int, (int, int)) vt1 = (1, 1 / 3, 3); // Error [CS0029]

        decimal FooDecimal(decimal d)
        {
            return d;
        }

        bar = (1, FooInt(1 / 3));

        fooBar = (1, FooInt(1 / 3));

        int FooInt(int d)
        {
            return d;
        }
    }

    public decimal M(int x) => x;
    public decimal M((int, int) x) => x.Item1;

    public void FNs()
    {
        var a = M(1 / 3);
        decimal a2 = 1 / 3 == 1 ? 1 / 3 : 1 / 3; // FN
        (decimal, decimal) a5 = (1, M(1 / 3)); // FN
        (decimal, decimal) a6 = (1, 1 / 3 + 1 / 3); // FN
        var a9 = M((1, 1 / 3)); // FN
    }

    public async Task FNAsync()
    {
        decimal a4 = await Task.FromResult(1 / 3); // FN
    }

}

public class NativeIntChecks
{
    decimal result;
    Func<decimal> decimalFunc;

    IntPtr intPtr1 = 2;
    IntPtr intPtr2 = 3;

    UIntPtr uIntPtr1 = 2;
    UIntPtr uIntPtr2 = 3;

    nint nint1 = 2;
    nint nint2 = 3;

    nuint nuint1 = 2;
    nuint nuint2 = 3;

    void AssignmentChecks()
    {
        result = intPtr1 / intPtr2; // Noncompliant
        result = uIntPtr1 / uIntPtr2; // Noncompliant
        result = nint1 / nint2;  // Noncompliant
        result = nuint1 / nuint2; // Noncompliant
        result = intPtr1 / nint1; // Noncompliant
        result = nint1 / intPtr1; // Noncompliant
        result = uIntPtr1 / nuint1; // Noncompliant
        result = nuint1 / uIntPtr1; // Noncompliant

        result = (decimal)intPtr1 / intPtr2; // Compliant
        result = (decimal)uIntPtr1 / uIntPtr2; // Compliant
        result = (decimal)nint1 / nint2;  // Compliant
        result = (decimal)nuint1 / nuint2; // Compliant
        result = (decimal)intPtr1 / nint1; // Compliant
        result = (decimal)nint1 / intPtr1; // Compliant
        result = (decimal)uIntPtr1 / nuint1; // Compliant
        result = (decimal)nuint1 / uIntPtr1; // Compliant
    }

    void MethodArgumentChecks()
    {
        MethodAcceptingDecimal(intPtr1 / intPtr2); // Noncompliant
        MethodAcceptingDecimal(uIntPtr1 / uIntPtr2); // Noncompliant

        MethodAcceptingDecimal((decimal)intPtr1 / intPtr2); // Compliant
        MethodAcceptingDecimal((decimal)uIntPtr1 / uIntPtr2); // Compliant

        decimal MethodAcceptingDecimal(decimal arg) => arg;
    }

    void FuncReturnChecks()
    {
        decimalFunc = () => intPtr1 / intPtr2; // Noncompliant
        decimalFunc = () => uIntPtr1 / uIntPtr2; // Noncompliant

        decimalFunc = () => (decimal)intPtr1 / intPtr2; // Compliant
        decimalFunc = () => (decimal)uIntPtr1 / uIntPtr2; // Compliant
    }

    decimal MethodReturnChecks()
    {
        return intPtr1 / intPtr2; // Noncompliant
        return uIntPtr1 / uIntPtr2; // Noncompliant

        return (decimal)intPtr1 / intPtr2; // Compliant
        return (decimal)uIntPtr1 / uIntPtr2; // Compliant
    }
}

interface IMyInterface
{
    static virtual void StaticVirtualMethod()
    {
        decimal dec = 3 / 2; // Noncompliant
    }
}
