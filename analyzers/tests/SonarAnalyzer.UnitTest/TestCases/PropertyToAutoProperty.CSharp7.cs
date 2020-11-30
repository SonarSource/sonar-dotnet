using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class PropertyToAutoProperty
    {
        private int field;

        public int Property01 //Noncompliant
        {
            get => field;
            set => field = value;
        }
    }

    public class VolatileField
    {
        private volatile int field;

        public int Property01 // Compliant - cannot have volatile autoproperty
        {
            get => field;
            set => field = value;
        }
    }
}
