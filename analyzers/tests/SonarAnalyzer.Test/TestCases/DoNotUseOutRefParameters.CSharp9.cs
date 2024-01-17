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
        bool TryMethod2(out string value);
    }

    public abstract record AbstractRecord
    {
        public abstract void Method1(out string value); // Noncompliant
        public abstract void Method2(ref string value); // Noncompliant
        public abstract bool TryMethod2(out string value);
    }

    public record OverridenRecord : AbstractRecord
    {
        public override void Method1(out string value) { value = "a"; }
        public override void Method2(ref string value) { }
        public override bool TryMethod2(out string value) { value = "a"; return true; }
    }

    internal record InternalRecord
    {
        public void Method1(out string value) { value = "a"; }
        public void Method2(ref string value) { }
        public bool TryMethod2(out string value) { value = "a"; return true; }
    }

    public record Record
    {
        public void Method1(out string value1, out string value2) { value1 = "a"; value2 = "a"; }
//                          ^^^
//                                             ^^^ Noncompliant@-1

        public void Method2(ref string value) { }// Noncompliant

        public bool TryMethod2(out string value) { value = "a"; return true; }
    }
}
