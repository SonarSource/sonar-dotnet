using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class UseCurlyBraces
    {
        public UseCurlyBraces(object obj)
        {
            if (false) ; // Noncompliant
//          ^^
            for (int i = 0; i < 10; i++) ; // Noncompliant
            foreach (int i in new List<int>()) ; // Noncompliant
            while (false) ; // Noncompliant
            do; while (false); // Noncompliant {{Add curly braces around the nested statement(s) in this 'do' block.}}

            if (false)
            {
            }
            else; // Noncompliant

            if (false) // Noncompliant
                if (false)
                {
                }

            if (false) { }

            if (false) { }
            else if (false) { }

            for (int i = 0; i < 10; i++) { }
            while (false) { }
            do { } while (false);

            if (obj is string str)
            {
                Console.WriteLine(str);
            }
        }
    }
}
