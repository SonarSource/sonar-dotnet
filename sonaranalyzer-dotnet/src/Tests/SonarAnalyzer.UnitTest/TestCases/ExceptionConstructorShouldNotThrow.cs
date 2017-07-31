using System;

namespace Tests.Diagnostics
{
    class MyException : Exception
    {
        public MyException()
        {
            throw new Exception(); // Noncompliant {{Avoid throwing exception in this constructor.}}
//          ^^^^^^^^^^^^^^^^^^^^^^
        }
    }

    class MyException2 : Exception
    {
        public MyException2()
        {

        }

        public MyException2(int i)
        {
            if (i == 42)
            {
                throw new Exception(); // Noncompliant
            }
        }
    }

    class MyException3 : Exception
    {
        public MyException3(int i)
        {
            if (i == 42)
            {
                throw new Exception(); // Noncompliant
            }
            else
            {
                throw new ArgumentException(); // Secondary
            }
        }
    }

    class SubException : MyException
    {
        public SubException()
        {
            throw new FieldAccessException(); // Noncompliant
        }
    }

    class Something
    {
        public Something()
        {
            throw new Exception(); // Compliant
        }
    }
}
