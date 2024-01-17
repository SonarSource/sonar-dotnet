class SecondWithIdentationRoot
{
    void While(bool condition)
    {
        while (condition)
            Act.First();

        Act.Second(); // Compliant
    }

    void For(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
            Act.First(args[i]);

        Act.Second(); // Compliant
    }

    void ForEach(string[] args)
    {
        foreach (var arg in args)
            Act.First(arg);

        Act.Second(); // Compliant
    }

    void If(bool condition)
    {
        if (condition)
            Act.First();

        Act.Second(); // Compliant
    }

    void Else(bool condition)
    {
        if (condition)
            Act.Other();

        else
            Act.First();

        Act.Second(); // Compliant
    }

    void ElseIf(bool condition)
    {
        if (condition)
            Act.Other();

        else if (condition)
            Act.First();

        Act.Second(); // Compliant
    }
}

class SecondWithIdentationFirst
{
    void While(bool condition)
    {
        while (condition)
            Act.First();
        //  ^^^^^^^^^^^^ Secondary
            Act.Second();
        //  ^^^^^^^^^^^^^
    }

    void For(string[] args)
    {
        for (var i = 0; i < args.Length; i++)
            Act.First(args[i]); // Secondary
            Act.Second(); // Noncompliant {{This line will not be executed in a loop; only the first line of this 2-line block will be. The rest will execute only once.}}
    }

    void ForEach(string[] args)
    {
        foreach (var arg in args)
            Act.First(arg); // Secondary
            Act.Second(); // Noncompliant {{This line will not be executed in a loop; only the first line of this 2-line block will be. The rest will execute only once.}}
    }

    void If(bool condition)
    {
        if (condition)
            Act.First(); // Secondary
            Act.Second(); // Noncompliant {{This line will not be executed conditionally; only the first line of this 2-line block will be. The rest will execute unconditionally.}}
    }

    void Else(bool condition)
    {
        if (condition)
            Act.Other();

        else
            Act.First(); // Secondary
            Act.Second(); // Noncompliant {{This line will not be executed conditionally; only the first line of this 2-line block will be. The rest will execute unconditionally.}}
    }

    void ElseIf(bool condition)
    {
        if (condition)
            Act.Other();

        else if (condition)
            Act.First(); // Secondary
            Act.Second(); // Noncompliant {{This line will not be executed conditionally; only the first line of this 2-line block will be. The rest will execute unconditionally.}}
    }
}

class FirstAndSecondOneSameLine
{
    void While(bool condition)
    {
        while (condition) Act.First(); Act.Second();
        //                ^^^^^^^^^^^^ Secondary
        //                             ^^^^^^^^^^^^^ @-1
    }

    void For(string[] args)
    {
        for (var i = 0; i < args.Length; i++) Act.First(args[i]); Act.Second();
        //                                    ^^^^^^^^^^^^^^^^^^^ Secondary
        //                                                        ^^^^^^^^^^^^^ @-1
    }

    void ForEach(string[] args)
    {
        foreach (var arg in args) Act.First(arg); Act.Second();
        //                        ^^^^^^^^^^^^^^^ Secondary
        //                                        ^^^^^^^^^^^^^ @-1
    }

    void If(bool condition)
    {
        if (condition) Act.First(); Act.Second();
        //             ^^^^^^^^^^^^ Secondary
        //                          ^^^^^^^^^^^^^ @-1
    }

    void Else(bool condition)
    {
        if (condition) Act.Other();
        else Act.First(); Act.Second();
        //   ^^^^^^^^^^^^ Secondary
        //                ^^^^^^^^^^^^^ @-1
    }

    void ElseIf(bool condition)
    {
        if (condition) Act.Other();
        else if (condition) Act.First(); Act.Second();
        //                  ^^^^^^^^^^^^ Secondary
        //                               ^^^^^^^^^^^^^ @-1
    }
}

class FirstOneSameLineAndSecondWithIndentation
{
    void While(bool condition)
    {
        while (condition) Act.First(); // Secondary
            Act.Second(); // Noncompliant
    }

    void For(string[] args)
    {
        for (var i = 0; i < args.Length; i++) Act.First(args[i]); // Secondary
            Act.Second(); // Noncompliant
    }

    void ForEach(string[] args)
    {
        foreach (var arg in args) Act.First(arg); // Secondary
            Act.Second(); // Noncompliant
    }

    void If(bool condition)
    {
        if (condition) Act.First(); // Secondary
            Act.Second(); // Noncompliant
    }

    void Else(bool condition)
    {
        if (condition) Act.Other();
        else Act.First(); // Secondary
            Act.Second(); // Noncompliant
    }

    void ElseIf(bool condition)
    {
        if (condition) Act.Other();
        else if (condition) Act.First(); // Secondary
            Act.Second(); // Noncompliant
    }
}

class Nested
{
    void While(bool condition)
    {
        while (true)
            while (condition) Act.First(); // Secondary
                Act.Second(); // Noncompliant
    }

    void For(string[] args)
    {
        for (var ii = 0; ii < args.Length; ii++)
            for (var i = 0; i < args.Length; i++) Act.First(args[i]); // Secondary
                Act.Second(); // Noncompliant
    }

    void ForEach(string[] args)
    {
        foreach (var _ in args)
            foreach (var arg in args) Act.First(arg); // Secondary
                Act.Second(); // Noncompliant
    }

    void If(bool condition)
    {
        if (condition)
            if (condition) Act.First(); // Secondary
                Act.Second(); // Noncompliant
    }
}

class SecondPartIsPartOfIfStructure
{
    void Else(bool condition)
    {
        if (condition)
            Act.First();
            else Act.Second(); // Compliant
    }
    void ElseIf(bool condition)
    {
        if (condition)
            Act.First();
            else if(condition) Act.Second(); // Compliant
    }
}

class SecondPartOfOtherScope
{
    void LowerElseIf(bool condition)
    {
        if (condition)
            if (condition)
                Act.First("if", "if");
            else
                Act.First("if", "else");
        else if (condition)  // Compliant
            Act.First("else if");
    }
    void LowerElse(bool condition)
    {
        if (condition)
            if (condition)
                Act.First("if", "if");
            else
                Act.First("if", "else");
        else Act.First("else"); // Compliant
    }
    void If(bool condition)
    {
        while (condition)
            if(condition)
                Act.First("while");
        if (condition) Act.First("while", "if");  // Compliant
    }
}

class Comments
{
    int BlockComment(int n)
    {
        while (true)
            while (true)
                n++; /* first */ // Secondary
        /*   */ return n; // Noncompliant
    }

    void BlockComment(bool condition)
    {
        if (condition)
            Act.First(); // Secondary
            /* Comment in the middle */
            Act.Second(); // Noncompliant
    }

    void LineComment(bool condition)
    {
        if (condition)
            Act.First(); // Secondary
            // Commented line
            Act.Second(); // Noncompliant
    }
}

class EmptyLines
{
    void HaveNoEffect(bool condition)
    {
        if (condition)
            Act.First(); // Secondary

            Act.Second(); // Noncompliant
    }
}

class EmptyStatements // Compliant, we ignore cases where at least one of the statements is empty
{
    public void First(bool condition)
    {
        if (condition) ; Act.Second();
    }
    public void Second(bool condition)
    {
        if (condition) Act.First(); ;
    }
    public void Both(bool condition)
    {
        if (condition) ; ;
    }
    public void BlockComment(bool condition)
    {
        if (condition) /* comment */; Act.Second();
    }
}

class Other
{
    void FirstSameLineSecondTooFar(bool condition)
    {
        if (condition) Act.First(); // Secondary
                            Act.Second(); // Noncompliant
    }

    int WithReturnStatement(bool condition)
    {
        while (condition)
            Act.First();
        //  ^^^^^^^^^^^^ Secondary
            return Act.Second();
        //  ^^^^^^^^^^^^^^^^^^^^
    }

    void ZeroIdentation(bool condition)
    {
        if (condition)
Act.First(); // Secondary
Act.Second(); // Noncompliant
    }

    void DesendingIndentation(bool condition)
    {
                if (condition)
            Act.First();
        Act.Second(); // Compliant, although inconvenient.
    }

    void ShiftedIndentation(bool condition)
    {
                if (condition)
        Act.First(); // Secondary
            Act.Second(); // Noncompliant
    }

    void AlternativeIndentation(bool condition)
    {
        try {
          if (condition)
            Act.First(); // This statement is aligned with the '{' of the try on purpose to fix https://github.com/SonarSource/sonar-dotnet/issues/264
        }
        finally { }
    }

    void LimitedIndentation(bool condition)
    {
        if (condition)
          Act.First();

        Act.Second();
    }

    void LimitedIndentationSameLine(bool condition)
    {
        if (condition) Act.First(); // Secondary
         Act.Second(); // Noncompliant
    }

    void IfElseWithAlternativeSpacing(bool condition)
    {
        var n = 0;
        if (condition)
            n = 2;
        else
        if (condition)
            n = 2;
        else
        if (condition)
            n = 2;
        else
        if (condition)
            n = 2;
        n = 4; // Compliant
    }

    public override bool Equals(object obj)
    {
        if (obj is null) return false;
        return obj is Other other && Equals(other); // Compliant
    }

    public bool Equals(Other other) { return other != null; }
}
class DoesNotCrash
{
    void OnMissingStatement(bool condition)
    {
        if (condition)
            // Error@-1 [CS1002] ; expected
            // Error@-2 [CS1525] Invalid expression term
    }
}

class Act
{
    public static void First(params string[] args) { }
    public static int Second() { return 42; }
    public static void Other() { }
}
