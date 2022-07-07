using System;
using System.Threading.Tasks;

public struct S
{
    public decimal Property { get; set; }

    public void M()
    {
        (Property, var _) = (3 / 2, 3 / 2);    // Noncompliant

        (decimal x, var y) = (1 / 3, 2);      // Noncompliant
        (decimal xx, var (yy, zz)) = (1 / 3, (2, 1 / 3)); // Noncompliant
        (decimal xxx, var (yyy, zzz)) = (1, (2, 1 / 3));
        (decimal xxxx, var (yyyy, zzzz)) = (1, (2, 1 / 3)); // FN


        var (a, b) = (1 / 3, 2);
        var (a1, (b1, c1)) = (1, (1 / 3, 2));
        var d = (1 / 3, 2);
        (int, int) d2 = (1 / 3, 2); 
        (decimal, decimal) d3 = (1 / 3, 2); // Noncompliant
        (int, (decimal, (int, decimal))) nested = (3, (1 / 3, (0, 1 / 3))); // Noncompliant [issue1, issue2]

        (decimal, decimal) d4 = (1 / 3, 2); // Noncompliant
        (decimal, decimal) d5 = (1 / 3, 1 / 3); // Noncompliant [issue3, issue4]
        (decimal d6, decimal d7) = (1 / 3, 1 / 3); // Noncompliant [issue5, issue6]

        var bar = (1, FooDecimal(1 / 3)); // Noncompliant

        (int, decimal) fooBar;
        fooBar = (1, FooDecimal(1 / 3)); // Noncompliant

        (int, ValueTuple<int, int>) vt = (1, (1 / 3, 3)); // FN
        (int, (int, int)) vt1 = (1, 1 / 3, 3); // Error

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

    public int M(int x) => x;
    public int M((int, int) x) => x.Item1;
    public async Task TestAsync()
    {
        var a = M(1 / 3);
        var a2 = 1 / 3 == 1 ? 1 / 3 : 1 / 3;
        var a4 = await Task.FromResult(1 / 3);
        var a5 = (1, M(1 / 3));
        var a6 = (1, 1 / 3 + 1 / 3);
        var a9 = M((1, 1 / 3));
    }
}
