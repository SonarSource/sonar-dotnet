using System;

namespace Tests.Diagnostics
{
    record Program
    {
        public void Foo() => throw new Exception(); // Compliant

        public Program()
        {
            throw new Exception(); // Compliant, ctor covered by S3877
        }

        ~Program()
        {
            throw new Exception(); // Noncompliant {{Remove this 'throw' statement.}}
//          ^^^^^^^^^^^^^^^^^^^^^^

            void Inner()
            {
                throw new Exception(); // Noncompliant
            };

            try
            {
                throw new Exception(); // Noncompliant, generally a bad idea to throw and catch in the same method
            }
            catch (Exception)
            {
            }

            try
            {
                Foo();
            }
            catch (Exception)
            {
                throw; // Noncompliant, rethrowing has the same effect as throwing
            }
        }
    }

    record C
    {
        ~C() => throw new Exception(); // Noncompliant
    }
}
