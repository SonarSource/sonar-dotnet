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

        var bar = (1, Foo(1 / 3)); // Noncompliant

        (int, decimal) fooBar;
        fooBar = (1, Foo(1 / 3)); // Noncompliant

        decimal Foo(decimal d)
        {
            return d;
        }
    }
}
