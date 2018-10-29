using System;
using System.Collections.Generic;
using System.IO;

namespace Tests.Diagnostics
{
    public class VariableShadowsField
    {
        public int myField;
        public IDisposable myDisposableField;
        public int myField2;
        public int @int;
        public int MyField { get; set; }

        public void doSomething()
        {
            int myField = 0, // Noncompliant
//              ^^^^^^^
                other = 5;
            int @int = 42; // Noncompliant

            for (myField = 0; myField < 10; myField++) // Compliant
            {
            }
            for (int myField2 = 0; myField2 < 10; myField2++) // Noncompliant {{Rename 'myField2' which hides the field with the same name.}}
            {
            }

            using (var myField2 = new MemoryStream()) // Noncompliant
            {
            }
            using (var local = new MemoryStream()) // Compliant
            {
            }
            using (myDisposableField = new MemoryStream()) // Compliant
            {
            }

            foreach (var myField2 in new[] { 1, 2 }) // Noncompliant
            {
            }
            foreach (var local in new[] { 1, 2 }) // Compliant
            {
            }
        }

        class X
        {
            public int f;
        }

        public unsafe void doSomeUnsafe()
        {
            var x = new X();

            fixed (int* myField2 = &x.f) // Noncompliant
            {
            }
            fixed (int* local = &x.f) // Compliant
            {
            }
        }

        public void doSomethingElse(int MyField) // Compliant
        {
            this.MyField = MyField;
        }

        public VariableShadowsField(int myField)
        {
            this.myField = myField;
        }

        public static VariableShadowsField build(int MyField)
        {
            return null;
        }
    }
}
