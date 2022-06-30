using System;

public record struct S
{
    public S()
    {
        (var i, var j) = (0, 0); // Noncompliant [issue1, issue2]

        (int, int) x = (0, 0); // Noncompliant
        (int, int) y = (0, 0);
        var z = (0, 0); // Noncompliant
        y.Item1 = 2;

        var a = 0;
        (a, var b) = (0, 0); // Noncompliant {{Remove the unused local variable 'b'.}}
//              ^

        (a, var k) = (0, 0);
        (a, k) = (0, 0);

        (a, (k, var c)) = (0, (0, 0)); // Noncompliant
        (a, k) = (0, 0);

        (int, int) used;
        used.Item1 = 3;

        (int, int) notUsed; // Noncompliant
    }
}
