using System;
using System.IO;

namespace Tests.TestCases
{
    class CatchRethrow
    {
        private void doSomething(  ) { throw new NotSupportedException()  ; }
        public void Test()
        {
            var someWronglyFormatted =      45     ;
            doSomething();
            doSomething();
            doSomething();

            try
            {
                doSomething();
            }
            catch (ArgumentException)
            {
                throw;
            }
            catch
            {
                Console.WriteLine("");
                throw;
            }

            try
            {
                doSomething();
            }
            catch (NotSupportedException)
            {
                Console.WriteLine("");
                throw;
            }

            try
            {
                doSomething();
            }
            catch (ArgumentException) when (true)
            {
                throw;
            }
            catch (NotSupportedException)
            {
                Console.WriteLine("");
                throw;
            }

            try
            {
                doSomething();
            }
            finally
            {

            }

            try
            {
                doSomething();
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("");
                throw;
            }

            try
            {
                doSomething();
            }
            catch (ArgumentNullException)
            {
                throw;
            }
            catch (NotImplementedException)
            {
                Console.WriteLine("");
                throw;
            }
            catch (ArgumentException)
            {
                ;
                throw;
            }
        }
    }

    class CatchRethrowTemporaryContext
    {
        void OrdinaryUsing()
        {
            using (File.OpenRead("file"))
            {
                throw new InvalidOperationException();
            }
        }

        void LookAlikeMethods()
        {
            Impersonate();
            Run();
            Revert();
            throw new InvalidOperationException();
        }

        void SpecificCatchBeforeRethrow()
        {
            try
            {
                throw new InvalidOperationException();
            }
            catch (ArgumentException)
            {
            }
        }

        void Impersonate() { }

        void Run() { }

        void Revert() { }
    }

    // Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/8199
    public class Repro8199
    {
        public void SomeMethod() => throw new NotSupportedException();
        public bool LogException(Exception ex) => false;

        public void CatchWithFilter()
        {
            try
            {
                SomeMethod();
            }
            catch (Exception ex) when (LogException(ex))
            {
                throw;
            }
        }
    }
}
