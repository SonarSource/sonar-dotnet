using System;

namespace Tests.Diagnostics
{
    class Program
    {
        int Foo(string s)
        {
            try
            {
                return s.Length;
            }
            catch (NullReferenceException nre) // Noncompliant {{Make the dereference conditional on its not being null.}}
//                 ^^^^^^^^^^^^^^^^^^^^^^
            {
                throw;
            }
        }
    }
}
