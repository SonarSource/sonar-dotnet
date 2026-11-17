using System;

var condition = true;
int total = 0;
string data = "abc";

if (condition)
    DoTheThing();

if (condition)      // Noncompliant
DoTheThing();       //  Secondary

do                  // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'do'}}
total++;            // Secondary
while (total < 10);

while (total < 20)  // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'while'}}
total++;            // Secondary

for (int i = 0; i < 10; i++)    // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'for'}}
total++;                        // Secondary

foreach (char item in data)     // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'foreach'}}
total++;                        // Secondary

if (total < 100)    // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'if'}}
total = 100;        // Secondary
else                // Noncompliant {{Use curly braces or indentation to denote the code conditionally executed by this 'else'}}
total = 200;        // Secondary

void TopLevelMethod(bool condition)
{
    if (condition)  // Noncompliant
    DoTheThing();   // Secondary

    Foo();

    // Compliant
    if (condition)
        DoTheThing();

    Foo();
}

void DoTheThing() { }
void Foo() { }

void LabeledLoops(int[][] rows)
{
    outer: foreach (var row in rows)    // Compliant NET-4932, the body is indented under the label on the same line
        foreach (var cell in row)
            if (cell == 0)
                break outer;

    next: for (var i = 0; i < rows.Length; i++) // Compliant NET-4932
        foreach (var cell in rows[i])
            if (cell < 0)
                continue next;
}

void LabeledLoopNotIndented(int[][] rows)
{
    outer: foreach (var row in rows)    // Noncompliant
    foreach (var cell in row)           // Secondary
        if (cell == 0)
            break outer;
}

void LabeledStatements(bool condition, int total)
{
    first: if (condition)       // Compliant
        DoTheThing();

    second: if (condition)      // Noncompliant
    DoTheThing();               // Secondary

    third: while (total < 10)   // Compliant
        total++;

    fourth: while (total < 20)  // Noncompliant
    total++;                    // Secondary

    fifth: do                   // Compliant
        total++;
    while (total < 30);

    sixth: do                   // Noncompliant
    total++;                    // Secondary
    while (total < 40);

    seventh: if (condition)     // Compliant
        DoTheThing();
    else
        Foo();

    eighth: if (condition)      // Compliant
        DoTheThing();
    else                        // Noncompliant
    Foo();                      // Secondary

    ninth: if (condition)       // Compliant
        DoTheThing();
    else if (total > 0)
        Foo();

    tenth: if (condition)       // Compliant
        DoTheThing();
    else if (total > 0)         // Noncompliant
    Foo();                      // Secondary

    eleventh: for (var i = 0; i < 10; i++)  // Compliant
        total++;

    twelfth: for (var i = 0; i < 10; i++)   // Noncompliant
    total++;                                // Secondary
}

void LabeledStatementsInsideBlock(bool condition, int total)
{
    if (condition)
    {
        first: for (var i = 0; i < 10; i++)     // Compliant
            total++;

        second: for (var i = 0; i < 10; i++)    // Noncompliant
        total++;                                // Secondary
    }
    else
    {
        third: while (total < 10)               // Compliant
            total++;

        fourth: while (total < 20)              // Noncompliant
        total++;                                // Secondary
    }
}

void ElseFollowedByLabeledIf(bool condition)
{
    if (condition)
        DoTheThing();
    else outer: if (condition)  // Noncompliant, the label is the reference
                                // Error@-1 [CS1023] Embedded statement cannot be a declaration or labeled statement
        Foo();                  // Secondary
}

void StackedLabels(int[][] rows)
{
    a: b: for (var i = 0; i < rows.Length; i++) // Compliant
        foreach (var cell in rows[i])
            if (cell < 0)
                continue b;

    c: d: for (var i = 0; i < rows.Length; i++) // Noncompliant
    foreach (var cell in rows[i])               // Secondary
        if (cell < 0)
            continue d;
}

void LabelOnItsOwnLine(int[][] rows)
{
    first:
        foreach (var row in rows)       // Compliant
            foreach (var cell in row)
                if (cell == 0)
                    break first;

    second:
        foreach (var row in rows)       // Noncompliant, the loop line is the reference, not the label line
        foreach (var cell in row)       // Secondary
            if (cell == 0)
                break second;

    third:
    fourth: foreach (var row in rows)   // Compliant
        foreach (var cell in row)
            if (cell == 0)
                break fourth;
}

void LabelIndentedWithTabs(int[][] rows)
{
	first: foreach (var row in rows)    // Compliant
	    foreach (var cell in row)
	        if (cell == 0)
	            break first;

	second: foreach (var row in rows)   // Noncompliant
	foreach (var cell in row)           // Secondary
	    if (cell == 0)
	        break second;
}

void LabelAfterCodeOnSameLine(int total)
{
    total++; label: for (var i = 0; i < 10; i++)    // Noncompliant, the preceding statement is part of the indentation
        total++;                                    // Secondary

    total++; for (var i = 0; i < 10; i++)           // Noncompliant, same as the labeled case above
        total++;                                    // Secondary

    total++; other: for (var i = 0; i < 10; i++)    // Compliant
                 total++;
}

public interface IStaticInterfaceMethod
{
    static virtual string MyStaticVirtualMethod(bool condition)
    {
        if (condition)  // Noncompliant
        return "Noncompliant";   // Secondary

        if (condition) // Compliant
            return "Compliant";

        return string.Empty;
    }
}
