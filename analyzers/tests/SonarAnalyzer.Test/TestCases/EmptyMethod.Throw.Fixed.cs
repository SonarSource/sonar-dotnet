using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

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
            throw new NotSupportedException();
        }

        public void ConditionalCompilation()
        {
#if SomeThing
            Console.WriteLine();
#endif
        }

        public void ConditionalCompilationEmpty() // Fixed
        {
            throw new NotSupportedException();
#if SomeThing
#endif
        }

        public void EmptyRegionTrivia() // Fixed
        {
            throw new NotSupportedException();
            #region
            #endregion
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
            throw new NotSupportedException();
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
                throw new NotSupportedException();
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
}
