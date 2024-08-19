using System;

namespace Tests.Diagnostics
{
    class StaticFieldWrittenFromInstanceMember
    {
        private static int count = 0; // Secondary
        public int MyProperty
        {
            get { return myVar; }
            set
            {
                count >>>= 42; // Noncompliant
                myVar = value;
            }
        }

        private int myVar;
    }

}
