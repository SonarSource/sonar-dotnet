using System;

namespace Tests.Diagnostics
{
    public class EmptyStatement
    {
        public int MyField;

        public EmptyStatement()
        {
            ; // Noncompliant {{Remove this empty statement.}}
            ; // Noncompliant
            ; // Noncompliant
            ; // Noncompliant
            ; // Noncompliant
            Console.WriteLine();
            while (true)
                ; // Noncompliant
//              ^
        }
    }
}
