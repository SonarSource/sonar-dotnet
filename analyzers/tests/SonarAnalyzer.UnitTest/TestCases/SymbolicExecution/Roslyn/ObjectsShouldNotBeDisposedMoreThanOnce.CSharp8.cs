using System;
using System.IO;

namespace Tests.Diagnostics
{
    namespace CSharp8
    {
        public class Disposable : IDisposable
        {
            public void Dispose() { }
        }

        class UsingDeclaration
        {
            public void Disposed_UsingDeclaration()
            {
                using var d = new Disposable();
                d.Dispose(); // FIXME Non-compliant {{Refactor this code to make sure 'd' is disposed only once.}}
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
                using (s ??= new Disposable()) // FIXME Non-compliant
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
                d.Dispose(); // FIXME Non-compliant
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
                    d.Dispose(); // FIXME Non-compliant - FN: local functions are not supported by the CFG
                }

                static void LocalStaticFunction()
                {
                    var d = new Disposable();
                    d.Dispose();
                    d.Dispose(); // FIXME Non-compliant - FN: local functions are not supported by the CFG
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
                s.Dispose(); // FIXME Non-compliant
            }

            public void M2()
            {
                using var s = new Struct();

                s.Dispose(); // FIXME Non-compliant
            }
        }
    }
}
