using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class ExpressionBodyProperties
    {
        private int field;

        private int Property01
        {
            get => field;
            set => field = value; // Noncompliant
        }

        private int Property02
        {
            get => field; // Noncompliant
            set => field = value;
        }

        public void Method()
        {
            int x;

            x = Property01;
            Property02 = x;
        }
    }

    public class EmptyCtor
    {
        // That's invalid syntax, but it is still empty ctor and we should not raise for it, even if it is not used
        public EmptyCtor() => // Error [CS1525,CS1002]
    }
}
