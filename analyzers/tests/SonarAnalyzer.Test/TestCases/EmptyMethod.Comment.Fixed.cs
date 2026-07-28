using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Tests.Diagnostics
{
    public class EmptyMethod
    {
        void F2()
        {
            // Do nothing because of X and Y.
        }

        void F3()
        {
            Console.WriteLine();
        }

        [Conditional("DEBUG")]
        void F4()    // Fixed
        {
            // Method intentionally left empty.
        }

        public void ConditionalCompilation()
        {
#if SomeThing
            Console.WriteLine();
#endif
        }

        public void ConditionalCompilationEmpty() // Fixed
        {
            // Method intentionally left empty.
        }

        public void EmptyRegionTrivia() // Fixed
        {
            // Method intentionally left empty.
        }

        protected virtual void F5()
        {
        }

        extern void F6();

        [DllImport("avifil32.dll")]
        private static extern void F7();
    }

    public abstract class MyClass
    {
        public void F1()
        {
            // Method intentionally left empty.
        } // Fixed

        public abstract void F2();
    }

    public class MyClass5 : MyClass
    {
        public override void F2()
        {
        }
    }

    public interface IInterface
    {
        public void F1() { } // Compliant, implemented interface methods are virtual by default

        public virtual void F2() { }

        public abstract void F3();
    }

    public class WithProp
    {
        public string Prop
        {
            set
            {
                // Method intentionally left empty.
            } // Fixed
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7629
    public class Repro_7629
    {
        interface Interface_7629
        {
            void MyMethod();
        }

        class MyClass_7629 : Interface_7629
        {
            public void MyMethod() { } // Compliant
        }
    }

    interface FirstInterface
    {
        public void Explicit();
        public void SameMethod();
    }

    interface SecondInterface
    {
        public void SameMethod();
    }

    class TestClass : FirstInterface, SecondInterface
    {
        void FirstInterface.Explicit() { } // Compliant
        public void SameMethod() { } // Compliant
    }

    public class Awaitable : INotifyCompletion
    {
        public Awaitable GetAwaiter() => this;

        public void GetResult() { }         // Compliant - awaiter members are duck-typed and treated like interface implementations. https://sonarsource.atlassian.net/browse/NET-2935
        public bool IsCompleted => !true;
        public void OnCompleted(Action continuation) { }
    }

    // Community-reported shape: readonly struct awaiter using ICriticalNotifyCompletion.
    public readonly struct CriticalAwaiter : ICriticalNotifyCompletion
    {
        public CriticalAwaiter GetAwaiter() => this;
        public void GetResult() { }         // Compliant - awaiter member
        public bool IsCompleted => true;
        public void OnCompleted(Action continuation) { }
        public void UnsafeOnCompleted(Action continuation) { }
    }

    // A class that is not an awaiter (no IsCompleted / OnCompleted) is still flagged for an empty GetResult.
    public class NotAnAwaiter
    {
        public void GetResult()
        {
            // Method intentionally left empty.
        } // Fixed
    }

    // A static GetResult is not the duck-typed instance awaiter member, so it is still flagged.
    public class AwaiterWithStaticGetResult : INotifyCompletion
    {
        public AwaiterWithStaticGetResult GetAwaiter() => this;
        public static void GetResult()
        {
            // Method intentionally left empty.
        } // Fixed
        public bool IsCompleted => true;
        public void OnCompleted(Action continuation) { }
    }

    // Looks like an awaiter by shape but does not implement INotifyCompletion, so it is not a usable
    // awaiter (CS4027) and the empty GetResult is still flagged.
    public class AwaiterShapeWithoutInterface
    {
        public bool IsCompleted => true;
        public void GetResult()
        {
            // Method intentionally left empty.
        } // Fixed
        public void OnCompleted(Action continuation) => throw new NotImplementedException();
    }
}
