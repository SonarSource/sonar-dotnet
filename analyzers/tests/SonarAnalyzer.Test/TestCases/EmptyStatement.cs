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
                ; // Compliant - empty statement is the body of a loop, excluded from S1116
        }

        // loop bodies are excluded
        public void LoopBodyExclusions()
        {
            for (int i = 0; i < 10; i++) ; // Compliant
            while (true) ; // Compliant
            do ; while (true); // Compliant
        }

        public void EmptyStatementInsideBlock()
        {
            while (true)
            {
                ; // Noncompliant - empty statement inside a block, not the direct body
            }
        }

        public void EmptyStatementAfterLoop()
        {
            for (int i = 0; i < 10; i++)
            {
                Console.WriteLine(i);
            }
            ; // Noncompliant - standalone empty statement, not a loop body
        }
    }
}
