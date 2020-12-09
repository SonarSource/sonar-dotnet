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
            catch (NullReferenceException nre) // Noncompliant {{Do not catch NullReferenceException; test for null instead.}}
//                 ^^^^^^^^^^^^^^^^^^^^^^
            {
                throw;
            }
        }

        void Bar(Action doSomething, Action<NullReferenceException> logException)
        {
            try
            {
                doSomething();
            }
            catch (NullReferenceException nre) // Noncompliant
            {
                logException(nre);
                throw;
            }
        }

        int FooBar(string s)
        {
            try
            {
                return s.Length;
            }
            catch (Exception e) when (((e is NullReferenceException))) // Noncompliant
            {
                throw;
            }
        }
    }
}
