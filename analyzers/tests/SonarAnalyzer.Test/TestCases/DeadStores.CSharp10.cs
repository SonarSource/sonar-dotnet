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

static (int, (int , (int, int))) ReturnNestedTuple()
{
    return (1, (2, (3, 4)));
}

static int ReturnAnInt()
{
    return 10;
}

void DoStuffWithInts()
{
    int x = ReturnAnInt();
    (x, int y) = ReturnIntTuple(); // Noncompliant {{Remove this useless assignment to local variable 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
}

void MultipleAssignmentInSingleDeconstruction()
{
    int x = 1;
    (x, x, (x, x)) = (1, 2, (3, 4));
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^      {{Remove this useless assignment to local variable 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  @-1 {{Remove this useless assignment to local variable 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  @-2 {{Remove this useless assignment to local variable 'x'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^  @-3 {{Remove this useless assignment to local variable 'x'.}}
}

void ReAssignmentDeconstruction()
{
    int x = 1;
    (x, x, (x, x)) = (x, x, (x, x)); // Noncompliant [issue1, issue2, issue3, issue4]
}

void ReAssignmentDeconstructionFromMethodCall()
{
    (_, (var _, (int x, var _))) = ReturnNestedTuple();
    (x, _) = ReturnNestedTuple();    // Noncompliant
}

void DoStuffWithIntsAgain()
{
    int? x = null;
    (x, int y) = ReturnIntTuple();   // Noncompliant
}

void TwoAssigments()
{
    int a, b, c = 0;
    (a, (b, c)) = (1, (2, 3));
//  ^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant    {{Remove this useless assignment to local variable 'a'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant@-1 {{Remove this useless assignment to local variable 'b'.}}
//  ^^^^^^^^^^^^^^^^^^^^^^^^^  Noncompliant@-2 {{Remove this useless assignment to local variable 'c'.}}
}

void DoSimplerStuffWithIntsAgain()
{
    int x;
    (x, var y) = (1, 2); // Noncompliant
}

Action<int, int, int> StaticLambda() =>
    static (int a, int _, int _) =>
    {
        a = 200; // FN
        (a, int b) = ReturnIntTuple(); // FN
    };

void ReassignAfter()
{
    int x;
    (x, var y) = (1, 2); // Noncompliant
    x = 2;
    Console.WriteLine(x);
}
