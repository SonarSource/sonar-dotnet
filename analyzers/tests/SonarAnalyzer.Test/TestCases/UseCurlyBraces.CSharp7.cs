using System;
using System.Collections;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class UseCurlyBraces
    {
        public UseCurlyBraces(object obj)
        {
            if (obj is string str)
            {
                Console.WriteLine(str);
            }
            if (obj is string str2) // Noncompliant
                Console.WriteLine(str2);
        }
    }
}
