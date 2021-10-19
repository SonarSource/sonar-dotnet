using System;

// TopLevelStatements:
// This should do the trick: CheckForDeadStores(c, c.SemanticModel.GetDeclaredSymbol(c.Node), firstGlobalStatement)
// but registering for CompilationUnit triggers the analysis twice, causing duplicates.
var x = 100; // FN, we don't register for CompilationUnit yet.
(x, int y) = ReturnIntTuple(); // FN

static (int a, int b) ReturnIntTuple()
{
    return (1, 2);
}

static int ReturnAnInt()
{
    return 10;
}

void DoStuffWithInts()
{
    int x = ReturnAnInt(); // FN - Rule does not handle tuples yet.
    (x, int y) = ReturnIntTuple(); // FN - Rule does not handle tuples yet.
}

void DoStuffWithIntsAgain()
{
    int? x = null; // Compliant
    (x, int y) = ReturnIntTuple();
}

Action<int, int, int> StaticLambda() =>
    static (int a, int _, int _) =>
    {
        a = 200; // FN
        (a, int b) = ReturnIntTuple(); // FN
    };

