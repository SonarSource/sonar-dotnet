
using System;

public class NoncompliantCases
{
    public string Wrong()
        => "wrong"; // Noncompliant {{Place the arrow at the end of the previous line.}}
    //  ^^

    public string AloneOnLine()
        =>          // Noncompliant
        "wrong";

    public string NoWhitespace()
=> "wrong"; // Noncompliant

    public string TooMuchWhitespace()


        => "wrong"; // Noncompliant

    public string CommentInTheWay()
        // There's a comment around here
        // And here
        => "wrong"; // Noncompliant

    public int Region()
    #region Why would you do this?
        => 0;   // Noncompliant
    #endregion

    public int Directive()
#if SOME_DIRECTIVE
        => 0;   // Not analyzed
#else
        => 1;   // Noncompliant
#endif

    public void LocalFunction()
    {
        int Local()
            => 1;   // Noncompliant

        static int StaticLocal()
            => 1;   // Noncompliant
    }

    public int SwitchExpression(object arg)
    {
        return arg switch
        {
            EventArgs
                => 1,   // Noncompliant
            Exception ex
                => 2,   // Noncompliant
            _
                => 3    // Noncompliant
        };
    }

    public int NoToken(object arg)
    {
        return arg switch
        {
            EventArgs   // Error [CS1003] Syntax error, '=>' expected
            42          // Compliant, while Roslyn gives us the Token with correct kind, it has IsMissing=true
        };
    }

    public int Property
        => 42;      // Noncompliant

    public int Accessors
    {
        get
            => 42;  // Noncompliant

        set
            => throw new NotImplementedException(); // Noncompliant
    }

    public int InitAccessor
    {
        init
            => throw new NotImplementedException();      // Noncompliant
    }

    public event EventHandler Event
    {
        add
            => DoSomething();   // Noncompliant

        remove
            => DoSomething();   // Noncompliant
    }

    public NoncompliantCases()
        => throw new Exception("Constructor");  // Noncompliant

    public static int operator +(NoncompliantCases a, NoncompliantCases b)
        => 42; // Noncompliant

    public void DoSomething() { }
}


public class CompliantCases
{
    public string CommentInTheWay() =>
        // There's a comment around here
        // And here
        "good";

    public int Region() =>
    #region Why would you do this?
        0;
    #endregion

    public int Directive() =>
#if SOME_DIRECTIVE
        0;   // Not analyzed
#else
        1;
#endif

    public void LocalFunction()
    {
        int Local() => 1;

        static int StaticLocal() => 1;
    }

    public int SwitchExpression(object arg)
    {
        return arg switch
        {
            EventArgs => 1,
            Exception ex => 2,
            _ => 3
        };
    }

    public int Property => 42;

    public int Accessors
    {
        get => 42;
        set => throw new NotImplementedException();
    }

    public int InitAccessor
    {
        init => throw new NotImplementedException();
    }

    public event EventHandler Event
    {
        add => DoSomething();
        remove => DoSomething();
    }

    public CompliantCases() =>
        throw new Exception("Constructor");

    public static int operator +(CompliantCases a, CompliantCases b) =>
        42;

    public int Generic<T>() where T : struct
        => 0;   // Noncompliant

    public int WrongParenthesis(
        )
        => 0; // Noncompliant


    public void DoSomething() { }
}



public class ReturnValueSameLine
{
    public int CompliantSingleLine => 0;

    public int WithOneParameter(int a) => 0;

    public int WithMultilineParameters(int a,
                                       int b,
                                       int c) => 0;

    public int Generic<T>() where T : struct => 0;

    public int WrongParenthesis(
        ) => 0; // Compliant, this is another problem
}

public class ReturnValueNextLine
{
    public int CompliantSingleLine =>
        0;

    public int WithOneParameter(int a) =>
        0;

    public int WithMultilineParameters(int a,
                                       int b,
                                       int c) =>
        0;

    public int Generic<T>() where T : struct =>
        0;

    public int WrongParenthesis(
        ) =>
        0; // Compliant, this is another problem
}

public class Lambdas
{
    public void Method()
    {
        Method(x => x + 1);
        Method((x, y) => x + y);
        Method(() => 1 + 2);

        Method(x =>
            x + 1);
        Method((x, y) =>
            x + y);
        Method(() =>
            1 + 2);

        Method(x =>
        {
            return 0;
        });

        Method(x
            => x + 1);  // Noncompliant
        //  ^^
        Method((x, y)
            => x + y);  // Noncompliant
        Method(()
            => 1 + 2);  // Noncompliant
    }

    public void Method(Func<int> lambda) { }
    public void Method(Func<int, int> lambda) { }
    public void Method(Func<int, int, int> lambda) { }
}

public class Others
{
    public void Comparison(int a, int b)
    {
        if (a >= b)
        {
            return;
        }

        if (a
            >= b)   // Bad, but compliant with this rule
        {
            return;
        }

        if (a
            <= b)   // Bad, but compliant with this rule
        {
            return;
        }

        if (a
            < b)   // Bad, but compliant with this rule
        {
            return;
        }

        if (a
            < b)   // Bad, but compliant with this rule
        {
            return;
        }
    }

}
