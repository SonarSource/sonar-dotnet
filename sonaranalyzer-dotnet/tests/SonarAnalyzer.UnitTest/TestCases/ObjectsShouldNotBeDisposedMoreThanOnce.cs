using System;
using System.IO;

namespace Tests.Diagnostics
{
    public interface IInterface1 : IDisposable { }

    class Program
    {
        public void DisposedTwise()
        {
            var d = new Disposable();
            d.Dispose();
            d.Dispose(); // Noncompliant
        }

        public void DisposedTwise_Conditional()
        {
            IDisposable d = null;
            d = new Disposable();
            if (d != null)
            {
                d.Dispose();
            }
            d.Dispose(); // Noncompliant {{Refactor this code to make sure 'd' is disposed only once.}}
//          ^
        }

        public void DisposedTwise_Try()
        {
            IDisposable d = null;
            try
            {
                d = new Disposable();
                var x = d;
                x.Dispose();
            }
            finally
            {
                d.Dispose(); // Noncompliant {{Refactor this code to make sure 'd' is disposed only once.}}
            }
        }

        public void DisposedTwise_Array()
        {
            var a = new[] { new Disposable() };
            a[0].Dispose();
            a[0].Dispose(); // Compliant, we don't handle arrays
        }

        public void Disposed_Using_WithDeclaration()
        {
            using (var d = new Disposable()) // Noncompliant
            {
                d.Dispose();
            }
        }

        public void Disposed_Using_WithExpressions()
        {
            var d = new Disposable();
            using (d) // Noncompliant
            {
                d.Dispose();
            }
        }

        public void Disposed_Using3(Stream str)
        {
            using (var s = new FileStream("path", FileMode.Open)) // Noncompliant
//                     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            {
                using (var sr = new StreamReader(s))
                {
                }
            }

            Stream stream;
            using (stream = new FileStream("path", FileMode.Open)) // Noncompliant
//                 ^^^^^^
            {
                using (var sr = new StreamReader(stream))
                {
                }
            }

            using (str)
            {
                var sr = new StreamReader(str);
                using (sr) // Compliant, we cannot detect if 'str' was argument of the 'sr' constructor or not
                {
                }
            }
        }

        public void Disposed_Using4()
        {
            Stream s = new FileStream("path", FileMode.Open);
            try
            {
                using (var sr = new StreamReader(s))
                {
                    s = null;
                }
            }
            finally
            {
                if (s != null)
                {
                    s.Dispose();
                }
            }
        }

        public void Disposed_Using_Parameters(IDisposable param1)
        {
            param1.Dispose();
            param1.Dispose(); // Noncompliant
        }

        public void Close_ParametersOfDifferenceTypes(IInterface1 interface1, IDisposable interface2)
        {
            // Regression test for https://github.com/SonarSource/sonar-csharp/issues/1038
            interface1.Dispose(); // ok, only called once on each parameter
            interface2.Dispose();
        }

        public void Close_ParametersOfSameTypes(IInterface1 instance1, IInterface1 instance2)
        {
            // Regression test for https://github.com/SonarSource/sonar-csharp/issues/1038
            instance1.Dispose();
            instance2.Dispose();
        }

        public void Close_OneParameterDisposedTwice(IInterface1 instance1, IInterface1 instance2)
        {
            instance1.Dispose();
            instance1.Dispose(); // Noncompliant
            instance1.Dispose(); // Noncompliant

            instance2.Dispose(); // ok - only disposed once
        }

    }

    public class Disposable : IDisposable
    {
        public void Dispose() { }
    }

    public class MyClass : IDisposable
    {
        public void Dispose() { }

        public void DisposeMultipleTimes()
        {
            Dispose();
            this.Dispose(); // Noncompliant
            Dispose(); // Noncompliant
        }

        public void DoSomething()
        {
            Dispose();
        }
    }
}
