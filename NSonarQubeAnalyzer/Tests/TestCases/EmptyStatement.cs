namespace Tests.Diagnostics
{
    using System;

    public class EmptyStatement
    {
        public int MyField;
        ;

        public EmptyStatement()
        {
            ; // Noncompliant
            Console.WriteLine();
        }
    }
}
