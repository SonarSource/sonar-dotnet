using System.Collections.Generic;

class MyClass
{
    void Compliant(int a, int b, int c, object o, List<int> list)
    {
        _ = a + b;
        _ =
            a + b;
        c +=
            a - b;
        c -=
            a - b;
        c *=
            a - b;
        c /=
            a - b;
        c %=
            a - b;
        o ??=
            new
                object();
        c <<=
            a - b;
        c >>=
            a - b;
        c >>>=
            a - b;
        c |=
            a - b;
        c &=
            a - b;
        c ^=
            a - b;
        o?
            .
            ToString();
        o!
            .
            ToString();

        o.ToString
            ();
        _ = list?
            [0];
        _ = list!
            [0];
    }

    void LogicalOperator(bool a, bool b)
    {
        _ = a && b;
        _ = a &&    // Noncompliant {{The '&&' operator should not be at the end of the line.}}
        //    ^^
            b;
        _ = a ||    // Noncompliant
            b;
        _ = a &     // Noncompliant
            b;
        _ = a |     // Noncompliant
            b;
        _ = a ^     // Noncompliant
            b;

        _ = a
            &&      // Noncompliant
            b;

        _ = a &&    // Noncompliant
            (b ||   // Noncompliant
            a) &&   // Noncompliant
            b;
    }

    void Ternary(bool a, bool b)
    {
        _ = a ? 1 : 2;
        _ = a ? 1 :     // Noncompliant
            2;
        _ = a ?         // Noncompliant
            1 :         // Noncompliant
            2;

        _ = a
            && // Noncompliant
            b
            ? // Noncompliant {{The '?' operator should not be at the end of the line.}}
            1
            : // Noncompliant {{The ':' operator should not be at the end of the line.}}
            2;
    }

    void Coelesce(object o1, object o2)
    {
        _ = o1 ?? o2;
        _ = o1 ??   // Noncompliant
            o2;
        _ = o1 ??   // Noncompliant
            o2;
        _ = o1
            ??      // Noncompliant
            o2;

        _ = o1?
            .ToString();
    }

    void AsIs(object o)
    {
        _ = o as string;
        _ = o as // Noncompliant
            string;
        _ = o
            as // Noncompliant
            string;
        _ = o is string;
        _ = o is // Noncompliant
            string;
        _ = o
            is // Noncompliant
            string;
    }

    void Pattern(object o1, object o2)
    {
        _ = o1 is int or float;
        _ = o1 is // Noncompliant
            int or float;
        _ = o1 is int or // Noncompliant
            float;

        _ = o1 is string { Length: > 5 } and // Noncompliant {{The 'and' operator should not be at the end of the line.}}
            { Length: < 10 };

        _ = o2 is string { Length: > 5 }
            and // Noncompliant
            { Length: < 10 };
    }

    void Arithmetic(int a, int b, string str)
    {
        _ = a + b;
        _ = a + // Noncompliant
            b;
        _ = a
            + // Noncompliant
            b;
        _ = a - b;
        _ = a - // Noncompliant
            b;
        _ = a
            - // Noncompliant
            b;
        _ = a * b;
        _ = a * // Noncompliant
            b;
        _ = a
            * // Noncompliant
            b;
        _ = a / b;
        _ = a / // Noncompliant
            b;
        _ = a
            / // Noncompliant
            b;
        _ = a % b;
        _ = a % // Noncompliant
            b;
        _ = a
            % // Noncompliant
            b;

        _ = str + " text " + str;
        _ = str + // Noncompliant
            " text " + str;
        _ = str
            + " text " + // Noncompliant
            str;
    }

    void Comparison(int a, int b)
    {
        _ = a == b;
        _ = a == // Noncompliant
            b;
        _ = a
            == // Noncompliant
            b;
        _ = a != b;
        _ = a != // Noncompliant
            b;
        _ = a
            != // Noncompliant
            b;
        _ = a < b;
        _ = a < // Noncompliant
            b;
        _ = a
            < // Noncompliant
            b;
        _ = a > b;
        _ = a > // Noncompliant
            b;
        _ = a
            > // Noncompliant
            b;
        _ = a <= b;
        _ = a <= // Noncompliant
            b;
        _ = a
            <= // Noncompliant
            b;
        _ = a >= b;
        _ = a >= // Noncompliant
            b;
        _ = a
            >= // Noncompliant
            b;
    }

    void Shift(int a, int b)
    {
        _ = a << b;
        _ = a << // Noncompliant
            b;
        _ = a
            << // Noncompliant
            b;
        _ = a >> b;
        _ = a >> // Noncompliant
            b;
        _ = a
            >> // Noncompliant
            b;
        _ = a >>> b;
        _ = a >>> // Noncompliant
            b;
        _ = a
            >>> // Noncompliant
            b;
    }

    // Covered by SA1003
    void Unary(int a, bool b)
    {
        _ = a++
            + 1;
        _ = +a;
        _ = +
            a;
        _ = -a;
        _ = -
            a;
        _ = ~a;
        _ = ~
            a;
        _ = ++a;
        _ = ++
            a;
        _ = --a;
        _ = --
            a;
        _ = a++;
        _ = !b;
        _ = !
            b;
        _ = ^a;
        _ = ^
            a;

        unsafe
        {
            _ = &a;
            _ = &
                a;
            int *p = &a;
            _ = *p;
            _ = *
                p;
        }
    }

    void Range()
    {
        _ = 1..2;
        _ = 1.. // Noncompliant
            2;
        _ = 1
            .. // Noncompliant
            2;
    }
}
