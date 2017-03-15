using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IMyInterface
    {
        void Print2(string[] messages);
        void Print2(string[] messages, string delimiter = "\n");// Noncompliant;
//                                     ^^^^^^^^^^^^^^^^^^^^^^^
    }

    public class MyBase
    {
        public virtual void Print3(string[] messages) { }
        public virtual void Print3(string[] messages, string delimiter = "\n") { }// Noncompliant {{This method signature overlaps the one defined on line 15, the default parameter value can't be used.}}
    }

    public class MethodOverloadOptionalParameter : MyBase, IMyInterface
    {
        public override void Print3(string[] messages) { }
        public override void Print3(string[] messages, string delimiter = "\n") { }// Compliant; comes from base class

        public void Print2(string[] messages) { }
        public void Print2(string[] messages, string delimiter = "\n") { } // Compliant; comes from interface

        partial void Print(string[] messages);

        partial void Print(string[] messages) { }

        void Print(string[] messages, string delimiter = "\n") {} // Noncompliant;
        void Print(string[] messages,
            string delimiter = "\n", // Noncompliant {{This method signature overlaps the one defined on line 29, the default parameter value can only be used with named arguments.}}
            string b = "a" // Noncompliant {{This method signature overlaps the one defined on line 31, the default parameter value can't be used.}}
            ) {}
    }
}
