using System;

namespace Tests.Diagnostics
{
    class ValidProgram : IDisposable
    {
        static ValidProgram()
        {
            throw new NotImplementedException();
        }

        event EventHandler OnSomething
        {
            add
            {
                throw new NotImplementedException();
            }
            remove
            {
                throw new NotImplementedException();
            }
        }

        event EventHandler OnSomething1
        {
            add
            {
                throw new InvalidOperationException(); // Compliant
            }
            remove
            {
                throw new InvalidOperationException(); // Compliant
            }
        }

        event EventHandler OnSomething2
        {
            add
            {
                throw new NotSupportedException(); // Compliant
            }
            remove
            {
                throw new NotSupportedException(); // Compliant
            }
        }

        event EventHandler OnSomething3
        {
            add
            {
                throw new ArgumentException(); // Compliant
            }
            remove
            {
                throw new ArgumentException(); // Compliant
            }
        }

        public override bool Equals(object obj)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            throw new NotImplementedException();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ValidProgram a, ValidProgram b)
        {
            throw new NotImplementedException();
        }

        public static bool operator !=(ValidProgram a, ValidProgram b)
        {
            throw new NotImplementedException();
        }

        public static bool operator >=(ValidProgram a, ValidProgram b)
        {
            throw new NotImplementedException();
        }

        public static bool operator <=(ValidProgram a, ValidProgram b)
        {
            throw new NotImplementedException();
        }

        public static bool operator >(ValidProgram a, ValidProgram b)
        {
            throw new NotImplementedException();
        }

        public static bool operator <(ValidProgram a, ValidProgram b)
        {
            throw new NotImplementedException();
        }

        public static implicit operator byte(ValidProgram d)
        {
            throw new NotImplementedException();
        }
    }

    class ValidRethrowProgram : IDisposable
    {
        static ValidRethrowProgram()
        {
            try
            {
            }
            catch (Exception)
            {
                throw;
            }
        }

        event EventHandler OnSomething
        {
            add
            {
                try
                {
                }
                catch (Exception)
                {
                    throw;
                }
            }
            remove
            {
                try
                {
                }
                catch (Exception)
                {
                    throw;
                }
            }
        }

        public override bool Equals(object obj)
        {
            try
            {
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        public static bool operator ==(ValidRethrowProgram a, ValidRethrowProgram b) // Error [CS0216] - no != operator
        {
            try
            {
            }
            catch (Exception)
            {
                throw;
            }

            return true;
        }

        public void Dispose() { }
    }

    class InvalidProgram : IDisposable
    {
        static InvalidProgram()
        {
            throw new Exception(); // Noncompliant {{Remove this 'throw' statement.}}
//          ^^^^^^^^^^^^^^^^^^^^^^
        }

        event EventHandler OnSomething
        {
            add
            {
                throw new Exception(); // Noncompliant
            }
            remove
            {
                throw new Exception(); // Noncompliant
            }
        }

        public override bool Equals(object obj)
        {
            throw new Exception(); // Noncompliant
        }

        public override int GetHashCode()
        {
            throw new Exception(); // Noncompliant
        }

        public override string ToString()
        {
            throw new Exception(); // Noncompliant
        }

        public void Dispose()
        {
            throw new Exception(); // Noncompliant
        }

        public static bool operator ==(InvalidProgram a, InvalidProgram b)
        {
            throw new Exception(); // Noncompliant
        }

        public static bool operator !=(InvalidProgram a, InvalidProgram b)
        {
            throw new Exception(); // Noncompliant
        }

        public static bool operator >=(InvalidProgram a, InvalidProgram b)
        {
            throw new Exception(); // Noncompliant
        }

        public static bool operator <=(InvalidProgram a, InvalidProgram b)
        {
            throw new Exception(); // Noncompliant
        }

        public static bool operator >(InvalidProgram a, InvalidProgram b)
        {
            throw new Exception(); // Noncompliant
        }

        public static bool operator <(InvalidProgram a, InvalidProgram b)
        {
            throw new Exception(); // Noncompliant
        }

        public static implicit operator byte(InvalidProgram d)
        {
            throw new Exception(); // Noncompliant
        }
    }

    ref struct DisposableRefStruct
    {
        public void Dispose()
        {
            throw new Exception(); // Noncompliant
        }
    }

    class ArrowMethods : IDisposable
    {
        static ArrowMethods() => throw new Exception(); // Noncompliant
        public void Dispose() => throw new Exception(); // Noncompliant
        public static bool operator ==(ArrowMethods a, ArrowMethods b) => throw new Exception(); // Noncompliant
        public static bool operator !=(ArrowMethods a, ArrowMethods b) => throw new Exception(); // Noncompliant
        event EventHandler OnSomething
        {
            add => throw new Exception(); // Noncompliant
            remove => throw new Exception(); // Noncompliant
        }
        public static implicit operator byte(ArrowMethods d) => throw new Exception(); // Noncompliant

        private string name;
        public override string ToString() =>
            string.IsNullOrEmpty(name)
                ? name == "x"
                    ? throw new NotImplementedException()
                    : "y"
                : throw new ArgumentException("..."); // Noncompliant
    }

    class CompliantArrowMethods : IDisposable
    {
        static CompliantArrowMethods() => throw new NotImplementedException();
        public void Dispose() => throw new NotImplementedException();
        public static bool operator ==(CompliantArrowMethods a, CompliantArrowMethods b) => throw new NotImplementedException();
        public static bool operator !=(CompliantArrowMethods a, CompliantArrowMethods b) => throw new NotImplementedException();
        event EventHandler OnSomething
        {
            add => throw new InvalidOperationException();
            remove => throw new ArgumentException();
        }

        static void Foo() => throw new Exception();

        private string name;
        public override string ToString() =>
            string.IsNullOrEmpty(name)
                ? name == "x"
                    ? throw new NotImplementedException()
                    : "y"
                : throw new NotImplementedException("...");
    }

    class MultipleExceptions
    {
        public override string ToString()
        {
            if (Foo())
            {
                if (Foo())
                {
                    throw new Exception(); // Noncompliant
                }
                throw new Exception(); // FN only the first is reported
            }
            else
            {
                throw new Exception(); // FN only the first is reported
            }
        }
        bool Foo() => true;
    }

    class CodeCoverage : IDisposable
    {
        static CodeCoverage() => throw new UnknownException(); // Error [CS0246]
        public void Dispose()
        {
            throw new UnknownException(); // Error [CS0246]
        }
        public override bool Equals(object obj)
        {
            Dispose();
            return true;
        }
        public override int GetHashCode() => 0;
    }

    public struct S
    {
        static S()
        {
            throw new Exception(); // Noncompliant
        }
    }

}
