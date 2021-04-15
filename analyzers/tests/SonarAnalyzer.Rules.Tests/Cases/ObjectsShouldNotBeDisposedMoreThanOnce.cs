using System;
using System.IO;

namespace Tests.Diagnostics
{
    public interface IInterface1 : IDisposable { }

    class Program
    {
        public void DisposedTwice()
        {
            var d = new Disposable();
            d.Dispose();
            d.Dispose(); // Noncompliant
        }

        public void DisposedTwice_Conditional()
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

        public void DisposedTwice_Try()
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

        public void DisposedTwice_Array()
        {
            var a = new[] { new Disposable() };
            a[0].Dispose();
            a[0].Dispose(); // Compliant, we don't handle arrays
        }

        public void Dispose_Stream_LeaveOpenFalse()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Compliant
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: false))
            {
            }
        }

        public void Dispose_Stream_LeaveOpenTrue()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Compliant
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: true))
            {
            }
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

        public void Disposed_Using_Parameters(IDisposable param1)
        {
            param1.Dispose();
            param1.Dispose(); // Noncompliant
        }

        public void Close_ParametersOfDifferentTypes(IInterface1 interface1, IDisposable interface2)
        {
            // Regression test for https://github.com/SonarSource/sonar-dotnet/issues/1038
            interface1.Dispose(); // ok, only called once on each parameter
            interface2.Dispose();
        }

        public void Close_ParametersOfSameType(IInterface1 instance1, IInterface1 instance2)
        {
            // Regression test for https://github.com/SonarSource/sonar-dotnet/issues/1038
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

    class TestLoopWithBreak
    {
        public static void LoopWithBreak(System.Collections.Generic.IEnumerable<string> list, bool condition, IInterface1 instance1)
        {
            foreach (string x in list)
            {
                try
                {
                    if (condition)
                    {
                        instance1.Dispose(); // Noncompliant
                    }
                    break;
                }
                catch (Exception)
                {
                    continue;
                }
            }
        }
    }
}

namespace Tests.Diagnostics
{
    namespace CSharp8
    {
        class UsingDeclaration
        {
            public void Disposed_UsingDeclaration()
            {
                using var d = new Disposable();
                d.Dispose(); // Noncompliant {{Refactor this code to make sure 'd' is disposed only once.}}
            }
        }

        public class NullCoalescenceAssignment
        {
            public void NullCoalescenceAssignment_Compliant(IDisposable s)
            {
                s ??= new Disposable();
                s.Dispose();
            }

            public void NullCoalescenceAssignment_NonCompliant(IDisposable s)
            {
                using (s ??= new Disposable()) // Noncompliant
                {
                    s.Dispose();
                }
            }
        }

        public interface IWithDefaultMembers
        {
            void DoDispose()
            {
                var d = new Disposable();
                d.Dispose();
                d.Dispose(); // Noncompliant
            }
        }

        public class LocalStaticFunctions
        {
            public void Method(object arg)
            {
                void LocalFunction()
                {
                    var d = new Disposable();
                    d.Dispose();
                    d.Dispose(); // Compliant - FN: local functions are not supported by the CFG
                }

                static void LocalStaticFunction()
                {
                    var d = new Disposable();
                    d.Dispose();
                    d.Dispose(); // Compliant - FN: local functions are not supported by the CFG
                }
            }
        }

        public class DefaultLiteralDoesNotCrashRule
        {
            public void Method(object arg)
            {
                int i = default;
            }
        }

        public class TupleExpressionsDoNotCrashRule
        {
            public void Method(object myObject1, object myObject2)
            {
                var myTuple = (1, 2);
                (object a, object b) c = (1, null);
                (object d, object e) = (1, null);
                (myObject1, myObject2) = (1, null);
            }
        }

        public ref struct Struct
        {
            public void Dispose()
            {
            }
        }

        public class Consumer
        {
            public void M1()
            {
                var s = new Struct();

                s.Dispose();
                s.Dispose(); // Noncompliant
            }

            public void M2()
            {
                using var s = new Struct();

                s.Dispose(); // Noncompliant
            }
        }
    }
}
