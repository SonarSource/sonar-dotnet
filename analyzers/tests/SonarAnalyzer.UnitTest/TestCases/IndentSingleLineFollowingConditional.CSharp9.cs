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

    // Compliant slution
    if (condition)
        DoTheThing();

    Foo();
}

void DoTheThing() { }
void Foo() { }
