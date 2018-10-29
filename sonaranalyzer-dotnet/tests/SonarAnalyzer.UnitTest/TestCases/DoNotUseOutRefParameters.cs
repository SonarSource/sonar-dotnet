using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public interface IProgram
    {
        void Method1(out string value);
//                   ^^^ {{Consider refactoring this method in order to remove the need for this 'out' modifier.}}
        void Method2(ref string value);
//                   ^^^ {{Consider refactoring this method in order to remove the need for this 'ref' modifier.}}
        void TryMethod(out string value);
//                     ^^^ {{Consider refactoring this method in order to remove the need for this 'out' modifier.}}
        bool TryMethod1(ref string value);
//                      ^^^ {{Consider refactoring this method in order to remove the need for this 'ref' modifier.}}
        bool TryMethod2(out string value);
    }

    public abstract class AbstractProgram
    {
        public abstract void Method1(out string value); // Noncompliant
        public abstract void Method2(ref string value); // Noncompliant
        public abstract void TryMethod(out string value); // Noncompliant
        public abstract bool TryMethod1(ref string value); // Noncompliant
        public abstract bool TryMethod2(out string value);
    }

    public class OverridenProgram : AbstractProgram
    {
        public override void Method1(out string value) { value = "a"; }
        public override void Method2(ref string value) { }
        public override void TryMethod(out string value) { value = "a"; }
        public override bool TryMethod1(ref string value) { return false; }
        public override bool TryMethod2(out string value) { value = "a"; return true; }
    }

    internal class InternalProgram
    {
        public void Method1(out string value) { value = "a"; }
        public void Method2(ref string value) { }
        public void TryMethod(out string value) { value = "a"; }
        public bool TryMethod1(ref string value) { return false; }
        public bool TryMethod2(out string value) { value = "a"; return true; }
    }

    public class Program
    {
        public void Method1(out string value1, out string value2) { value1 = "a"; value2 = "a"; }
//                          ^^^
//                                             ^^^ Noncompliant@-1

        public void Method1(out string value) { value = "a"; } // Noncompliant

        public void Method2(ref string value) { }// Noncompliant

        public void TryMethod(out string value) { value = "a"; } // Noncompliant

        public bool TryMethod1(ref string value) { return true; } // Noncompliant

        public bool TryMethod2(out string value) { value = "a"; return true; }
    }
}
