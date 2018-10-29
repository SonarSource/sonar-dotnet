using System;

namespace Tests.Diagnostics
{
    class MyException : Exception
    {
        public MyException()
        {
            throw new Exception(); // Noncompliant {{Avoid throwing exceptions in this constructor.}}
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

    class MyException4 : Exception
    {
        public MyException4()
        {
            throw; // Noncompliant
            // Error@-1 [CS0156]
        }
    }

    class MyException5 : Exception
    {
        public MyException5()
        {
            var ex = new Exception();
            throw ex; // Noncompliant
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
