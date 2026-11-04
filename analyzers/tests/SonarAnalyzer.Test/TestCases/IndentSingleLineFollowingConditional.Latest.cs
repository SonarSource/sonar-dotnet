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
    outer: foreach (var row in rows)    // Noncompliant FP NET-4932
        foreach (var cell in row)       // Secondary
            if (cell == 0)
                break outer;

    next: for (var i = 0; i < rows.Length; i++) // Noncompliant FP NET-4932
        foreach (var cell in rows[i])           // Secondary
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
