﻿using System;
using System.Linq;

public class Sample
{
    private bool condition;
    private object first, middle, last;

    public void Method()
    {
        bool longVariableName;
        longVariableName = condition    // 0
        && true;        // Noncompliant {{Indent this operator at line position 13.}}

        longVariableName = condition   // 1
         && true;       // Noncompliant

        longVariableName = condition   // 2
          && true;      // Noncompliant

        longVariableName = condition   // 3
           && true;     // Noncompliant

        longVariableName = condition   // 4
            && true;

        longVariableName = condition   // 5
             && true;   // Noncompliant
        //   ^^^^^^^

        longVariableName = condition   // One of the branches is too far
            && true;

        longVariableName = condition   // One of the branches is too far
             && true;   // Noncompliant

        longVariableName = condition && true;
    }

    public bool ArrowNoncompliant() =>
        condition
            && true     // Noncompliant {{Indent this operator at line position 9.}}
            && true;    // Noncompliant

    public bool ArrowCompliant() =>
        condition
        && true
        && true
        && true;

    public object ArrowNullCoalescing() =>
        first
        ?? middle
        ?? middle
            ?? last;    // Noncompliant

    public bool ArrowNotInScope() =>
        condition && true;

    public object ArrowInInvocationArgument() =>
        Something(condition
            && true
            && true
                && true);   // Noncompliant

    public object ArrowInInvocationArgumentStartsOnNextLIne() =>
        Something(
            condition
            && true
            && true
                && true);   // Noncompliant

    public object ArrowInConstructorArgument() =>
        new Nullable<bool>(condition
                        && true     // Noncompliant
                            && true);

    public bool ReturnNoncompliant()
    {
        return condition
                && true; // Noncompliant, too much
    }

    public bool ReturnCompliant()
    {
        return condition
            && true
            && true;
    }

    public bool ReturnTernary()
    {
        return condition
            && true
                ? true
                : false;
    }

    public void Invocations()
    {
        Something(condition // This is bad already
        && true,            // Noncompliant {{Indent this operator at line position 13.}}
            "Some other argument");
        Something(condition     // This is bad already
                    && true,    // Noncompliant
            "Some other argument");
        global::Sample.Something(condition  // This is bad already
                && true     // Noncompliant {{Indent this operator at line position 13.}}
                && true,    // Noncompliant
            "Some other argument");
        global::Sample.Something(condition      // This is bad already
                                    && true,    // Noncompliant
            "Some other argument");
        Something(Something(Something(condition // This is bad already
                && true,    // Noncompliant {{Indent this operator at line position 13.}}
            "Some other argument")));
        Something(Something(Something(condition     // This is bad already
                                        && true,    // Noncompliant
            "Some other argument")));

        Something(
            condition
                && true,        // Noncompliant {{Indent this operator at line position 13.}}
            "Some other argument");
        Something(
            condition
            && true,
            "Some other argument");

        global::Sample.Something(       // Longer invocation name does not matter
            condition
            && true,
            "Some other argument");

        Something(Something(Something(  // This is bad already for other reasons
            condition
            && true
            && true,
            "Some other argument")));
    }

    public void ObjectInitializer()
    {
        _ = new WithProperty
        {
            Value = condition
                && true
            && true             // Noncompliant
                    && true     // Noncompliant
        };
    }

    public void CollectionExpression()
    {
        _ = string.Join(" ",
            [
                ""
            + "Too close"       // Noncompliant
                + "Good"
                    + "Too far" // Noncompliant
            ]);
        _ = string.Join(" ", [
            ""
        + "Too close"       // Noncompliant
            + "Good"
                + "Too far" // Noncompliant
            ]);
    }

    public void Lambdas(int[] list, int[] longer)
    {
        list.Where(x => condition
                    && true             // Noncompliant, too close
                            && true);   // Noncompliant, too close
        list.Where(x => condition
                                && true // Noncompliant, too far
                             && true);  // Noncompliant, too far
        list.Where(x => condition       // Simple lambda
                        && true
                        && true
                        && true);
        list.Where((x) => condition     // Parenthesized lambda
                            && true
                            && true);
        longer.Where(x => condition     // Simple lambda, longer name
                            && true);   // Compliant, as long as it's after the condition and aligned to the grid of 4
        longer.Where((x) => condition   // Parenthesized lambda, longer name
                            && true);   // Compliant, as long as it's after the condition and aligned to the grid of 4
        list.Select(xxxx => first
                            ?? middle
                            ?? middle
                                ?? last);   // Noncompliant
        list.Where(x =>
        {
            return condition
                && true;
        });
    }

    public void If()
    {
        if (condition
        && true             // Noncompliant {{Indent this operator at line position 13.}}
                && true)    // Noncompliant
        {
        }
        if (condition
            && true
            && true)
        {
        }
        else if (condition
            && true
                && true)    // Noncompliant
        {
        }
    }

    public void While()
    {
        while (condition
        && true             // Noncompliant {{Indent this operator at line position 13.}}
                && true)    // Noncompliant
        {
        }
        while (condition
            && true
            && true)
        {
        }
    }

    public void For()
    {
        for (var i = 0; condition
                    && true                 // Noncompliant {{Indent this operator at line position 25.}}
                            && true; i++)   // Noncompliant
        {
        }
        for (int i = 0; condition
                        && true; i++)
        { }
        for (int i = 0; condition
                        && true
                        && true; i++)
        { }
        for (int ii = 0; condition
                            && true
                            && true; ii++)
        { }
        for (int iii = 0; condition
                            && true
                            && true; iii++)
        { }
        for (int iiii = 0; condition
                            && true
                            && true; iiii++)
        { }
        for (int iiiii = 0; condition
                            && true
                            && true; iiiii++)
        { }
    }

    public void Nested()
    {
        _ = true
            || true
            || (false && true)
            || (false
            && true)            // Noncompliant {{Indent this operator at line position 17.}}
            || ((((false))
            && ((true))))       // Noncompliant {{Indent this operator at line position 17.}}
            || (((bool)(false))
            && ((bool)(true)))  // Noncompliant {{Indent this operator at line position 17.}}
            || (false
            && (true            // Noncompliant {{Indent this operator at line position 17.}}
            || false))          // Noncompliant {{Indent this operator at line position 17.}} This should be 21, but because the previous line is misplaced, it self-aligns with it to 17.
            || (false
                && (true
                || false));     // Noncompliant {{Indent this operator at line position 21.}} Once the previous line is fixed, this will be 21 as expected.
        _ = true
            || true
            || (false && true)
            || (false
                && true)
            || ((((false))
                && ((true))))
            || (((bool)(false))
                && ((bool)(true)))
            || (false
                && (true
                    || false));

        _ = true
            || (false
                && (condition is true
                or false));     // Noncompliant {{Indent this operator at line position 21.}}
        _ = true
            || (false
                && (condition is true
                    or false));
        _ = true
            || (false
                && (condition
                is true));      // Noncompliant {{Indent this operator at line position 21.}}
        _ = true
            || (false
                && (condition
                    is true));
    }

    public void ConditionalAccess(Builder builder)
    {
        builder?
            .Build(true
                || false
                    || false);           // Noncompliant

        builder?
            .Build(true
                || false
                    || false)?           // Noncompliant
            .Build(true
                || false
                    || false);           // Noncompliant
        builder?.Build()?.Build(true
            || false
                || false);               // Noncompliant
    }

    public void SwitchExpressions(object arg)
    {
        _ = arg switch
        {
            Exception someLongerName => condition
                && true
                    && true,            // Noncompliant
            _ => condition
                && true
                    && true             // Noncompliant
        };
    }

    public object Property =>
        first
        ?? middle
        ?? middle
            ?? last;  // Noncompliant

    public object AccessorCoalesce
    {
        get => first
            ?? middle
            ?? middle
                ?? last;  // Noncompliant
    }

    public bool Accessor
    {
        get => condition
            && true
                && true;    // Noncompliant
    }

    public static bool Something(bool arg, object another = null) => true;
}

class Operators
{
    object first, middle, last;

    void LogicalOperator(bool a, bool b)
    {
        _ = a
                && b;   // Noncompliant {{Indent this operator at line position 13.}}
        //      ^^^^
        _ = a
                || b;   // Noncompliant
        _ = a
                & b;    // Noncompliant
        _ = a
                | b;    // Noncompliant
        _ = a
                ^ b;    // Noncompliant
    }

    void Ternary()
    {
        _ = true
        ? 1 // Compliant, handled by T0025
        : 2;

        _ = true
        && true             // Noncompliant
                || false    // Noncompliant
            ? 1
            : 2;

        _ = true
            && true
            || false
            ? 1
            : 2;

        _ = true
            ? true
            && false            // Noncompliant
                    && false    // Noncompliant
            : false
            || true             // Noncompliant
                    || true;    // Noncompliant

        _ = true
            ? true
                && false
            : false
                || true;

        _ = true
            ? first
                ?? middle
                ?? middle
                    ?? last     // Noncompliant
            : null;
    }

    void Coelesce(object o1, object o2, object o3)
    {
        _ = o1
            ?? o2
            ?? o2
            ?? o2
                ?? o3;      // Noncompliant
    }

    void AsIs(object o)
    {
        _ = o
                as string;  // Noncompliant
        //      ^^^^^^^^^
        _ = o
                is string;  // Noncompliant
    }

    void Pattern(object o1, object o2)
    {
        _ = o1 is int
                or float;   // Noncompliant
        //      ^^^^^^^^

        _ = o1 is string { Length: > 5 }
                and { Length: < 10 };    // Noncompliant {{Indent this operator at line position 13.}}
    }

    void Arithmetic(int a, int b, string str)
    {
        _ = a
                + b;        // Noncompliant
        _ = a
                - b;        // Noncompliant
        _ = a
                * b;        // Noncompliant
        _ = a
                / b;        // Noncompliant
        _ = a
                % b;        // Noncompliant
        _ = str
                + "asdf";   // Noncompliant
    }

    void Comparison(int a, int b)
    {
        _ = a
                == b;   // Noncompliant
        _ = a
                != b;   // Noncompliant
        _ = a
                < b;    // Noncompliant
        _ = a
                > b;    // Noncompliant
        _ = a
                <= b;   // Noncompliant
        _ = a
                >= b;   // Noncompliant
    }

    void Shift(int a, int b)
    {
        _ = a
                << b;   // Noncompliant
        _ = a
                >> b;   // Noncompliant
        _ = a
                >>> b;  // Noncompliant
    }

    void Range()
    {
        _ = 1
                ..2;    // Noncompliant
        //      ^^^
    }
}

public class WithProperty
{
    public bool Value { get; set; }
}

public class Coverage
{
    [Obsolete("For" + " coverage", true is true or true)]   // Error [CS0182] An attribute argument must be a constant expression
    public void BinaryAndPatterns() { }

    [Obsolete(1..2)]    // Error [CS1503] Argument 1: cannot convert from 'System.Range' to 'string?'
    public void RangeOperator() { }
}

public class Builder
{
    public Builder Build(params object[] args) => this;
}
