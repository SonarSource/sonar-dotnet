using System;

public record struct S
{
    public S()
    {
        (var i, var j) = (0, 0); // Noncompliant [issue1, issue2]
        (var ii, (var jj, var kk)) = (0, (0, 0)); // Noncompliant [issue3, issue4, issue5]

        var (v, p) = (0, 0); // Noncompliant [issue6, issue7]
        var (vv, (pp, vvv)) = (0, (0, 0)); /// Noncompliant [issue8, issue9, issue10]

        (int, int) x = (0, 0); // Noncompliant
//                 ^
        (int, (int, int)) xx = (0, (0,0)); // Noncompliant
//                        ^^
        (int, int) y = (0, 0);
        y.Item1 = 2;

        var z = (0, 0); // Noncompliant

        var a = 0;
        (a, var b) = (0, 0); // Noncompliant {{Remove the unused local variable 'b'.}}
//              ^
        (a, (int, int) bb) = (0, (0, 0)); // Noncompliant
//                     ^^
        (a, var k) = (0, 0);
        (a, k) = (0, 0);

        (a, (k, var c)) = (0, (0, 0)); // Noncompliant
//                  ^
        (a, k) = (0, 0);

        (int, int) used;
        used.Item1 = 3;

        (int, (int, int)) usedNested;
        usedNested.Item1 = 3;

        (int, int) notUsed; // Noncompliant
//                 ^^^^^^^
        (int, (int, int)) notUsedNested; // Noncompliant
//                        ^^^^^^^^^^^^^
    }
}
