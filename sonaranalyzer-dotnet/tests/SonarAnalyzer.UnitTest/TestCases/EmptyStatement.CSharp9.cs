using System;

; // Noncompliant {{Remove this empty statement.}}
; // Noncompliant
Console.WriteLine();
while (true)
    ; // Noncompliant

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
