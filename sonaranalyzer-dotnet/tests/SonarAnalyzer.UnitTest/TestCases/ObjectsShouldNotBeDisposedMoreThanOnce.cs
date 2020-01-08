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

    public class UsingWithLeaveOpenArgument
    {
        public void LeaveOpenTrue1()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, true))
            {
                string str = null;
            }
        }

        public void LeaveOpenTrue2()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: true))
            {
                string str = null;
            }
        }

        public void LeaveOpenTrue3()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), leaveOpen: true, bufferSize: 1024))
            {
                string str = null;
            }
        }

        public void LeaveOpenTrue4()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream, new System.Text.UTF8Encoding(false), false, 1024, true))
            {
                string str = null;
            }
        }

        public void LeaveOpenTrue5()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamReader reader = new StreamReader(memoryStream, new System.Text.UTF8Encoding(false), bufferSize: 2014, leaveOpen: true, detectEncodingFromByteOrderMarks: false))
            {
                string str = null;
            }
        }

        public void LeaveOpenTrue6()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), leaveOpen: true, bufferSize: 1024))
            using (StreamReader reader = new StreamReader(memoryStream, new System.Text.UTF8Encoding(false), bufferSize: 2014, leaveOpen: true, detectEncodingFromByteOrderMarks: false))
            {
                string str = null;
            }
        }

        public void LeaveOpenTrue7()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Noncompliant
            using (StreamWriter writer = new StreamWriter(memoryStream))
            using (StreamReader reader = new StreamReader(memoryStream, new System.Text.UTF8Encoding(false), bufferSize: 2014, leaveOpen: true, detectEncodingFromByteOrderMarks: false))
            {
                string str = null;
            }
        }

        public void LeaveOpenTrue8()
        {
            using (MemoryStream memoryStream = new MemoryStream())
            using (StreamWriter writer = new StreamWriter(leaveOpen: true, stream: memoryStream, encoding: new System.Text.UTF8Encoding(false), bufferSize: 1024))
            {
                string str = null;
            }
        }

        public void LeaveOpenFalse1()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Noncompliant
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, false))
            {
                string str = null;
            }
        }

        public void LeaveOpenFalse2()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Noncompliant
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), 1024, leaveOpen: false))
            {
                string str = null;
            }
        }

        public void LeaveOpenFalse3()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Noncompliant
            using (StreamWriter writer = new StreamWriter(memoryStream, new System.Text.UTF8Encoding(false), leaveOpen: false, bufferSize: 1024))
            {
                string str = null;
            }
        }

        public void LeaveOpenFalse4()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Noncompliant
            using (StreamReader reader = new StreamReader(memoryStream, new System.Text.UTF8Encoding(false), true, 1024, false))
            {
                string str = null;
            }
        }

        public void LeaveOpenFalse5()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Noncompliant
            using (StreamReader reader = new StreamReader(memoryStream, new System.Text.UTF8Encoding(false), bufferSize: 2014, leaveOpen: false, detectEncodingFromByteOrderMarks: true))
            {
                string str = null;
            }
        }

        public void LeaveOpenFalse6()
        {
            using (MemoryStream memoryStream = new MemoryStream()) // Noncompliant
            using (StreamWriter writer = new StreamWriter(leaveOpen: false, stream: memoryStream, encoding: new System.Text.UTF8Encoding(false), bufferSize: 1024))
            {
                string str = null;
            }
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

            public void Implicit_Disposed_UsingDeclaration(Stream str)
            {
                using var s = new FileStream("path", FileMode.Open);
                using var sr = new StreamReader(s); // Noncompliant {{Refactor this code to make sure 's' is disposed only once.}}
            }

            public void Disposed_Using_Valid()
            {
                var s = new FileStream("path", FileMode.Open);
                try
                {
                    using var sr = new StreamReader(s);
                    s = null;
                }
                finally
                {
                    if (s != null)
                    {
                        s.Dispose();
                    }
                }
            }

            public void Mixed_Usings_Disposed(Stream str)
            {
                using var s = new FileStream("path", FileMode.Open);
                using (var sr = new StreamReader(s)) // Noncompliant {{Refactor this code to make sure 's' is disposed only once.}}
                {
                }
            }

            public void Mixed_Usings_ComplexCFG_Disposed(Stream str)
            {
                using var s = new FileStream("path1", FileMode.Open);
                using (var sr1 = new FileStream("path2", FileMode.Open))
                {
                    if (str != null)
                    {
                        using (var sr2 = new StreamReader(s)) // Noncompliant
                        {
                        }
                    }
                }
            }

            public void FileStream_Using_Null(Stream str)
            {
                using FileStream s = null;
                s.Dispose(); // Ok - s is null here, so it will raise a null pointer dereference instead
            }
        }

        public class NullCoalescenceAssignment
        {
            public void NullCoalescenceAssignment_NotNull(Stream s)
            {
                if (s == null)
                {
                    s ??= new FileStream("path", FileMode.Open);
                    using (var sr = new StreamReader(s))
                    {
                    }
                    s.Dispose(); // Noncompliant
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
