using System;
using System.Threading.Tasks;

public struct S
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
