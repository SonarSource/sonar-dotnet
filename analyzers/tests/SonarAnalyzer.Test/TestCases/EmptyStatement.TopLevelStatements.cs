using System;

; // Noncompliant {{Remove this empty statement.}}
; // Noncompliant
Console.WriteLine();
while (true)
    ; // Compliant: loop bodies are excluded

record R
{
    string P
    {
        init
        {
            ; // Noncompliant
        }
    }
}
