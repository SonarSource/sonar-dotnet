using System;
using System.Runtime.CompilerServices;
namespace Tests.Diagnostics
{
    class Program
    {
        public void Method1(string callerFilePath) { }
        public void Method2([CallerFilePath]string callerFilePath = "") { }
        public void Method3(string other, [CallerFilePath]string callerFilePath = "") { }
        public void Method4([CallerFilePath]string callerFilePath = "", string other = "") { } // Noncompliant {{Move 'callerFilePath' to the end of the parameter list.}}
//                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public void Method5([CallerFilePath]string callerFilePath = "", string other = "", [CallerLineNumber]int callerLineNumber = 0) { }
//                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        public void Method6(string first, [CallerFilePath]string callerFilePath = "", [CallerLineNumber]int callerLineNumber = 0, string other = "") { }
//                                                                                    ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
//                                        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^ @-1

        public void Method7([CallerFilePath]string callerFilePath = "",
//                          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            [CallerLineNumber]int callerLineNumber = 0,
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
            [CallerMemberName]string callerMemberName = "", string other = "") { }
//          ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^

        public void Method8([CallerMemberName]string callerMemberName = null,
            [CallerFilePath]string callerFilePath = null, [CallerLineNumber]int callerLineNumber = 0) { }

        public void Method9([System.Runtime.CompilerServices.CallerFilePath] string callerFilePath = "", string other = "") { } // Noncompliant

        public void Method10([System.Runtime.CompilerServices.CallerFilePathAttribute] string callerFilePath = "", string other = "") { } // Noncompliant

        public void Method11([CallerFilePathAttribute] string callerFilePath = "", string other = "") { } // Noncompliant

        public Program([CallerFilePath]string callerFilePath = "", string other = "") { } // Noncompliant
        public Program(int other, [CallerFilePath]string callerFilePath = "") { }

        public delegate void Del([CallerFilePath] string callerFilePath = "", string other = ""); // Noncompliant (see also https://github.com/dotnet/csharplang/discussions/7267#discussioncomment-6141445)
    }

    class BaseClass
    {
        public virtual void Method1(string callerFilePath, string other) { }
    }

    interface MyInterface
    {
        void Method2(string callerFilePath, string other);
    }

    class DerivedClass : BaseClass, MyInterface
    {
        public override void Method1([CallerFilePath]string callerFilePath = "", string other = "") // Compliant, method overriden
        {
        }

        public void Method2([CallerFilePath]string callerFilePath = "", string other = "") // Compliant, method from interface
        {
        }
    }
}
