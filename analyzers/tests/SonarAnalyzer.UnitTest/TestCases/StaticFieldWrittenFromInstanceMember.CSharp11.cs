using System;

namespace Tests.Diagnostics
{
    class StaticFieldWrittenFromInstanceMember
    {
        private static int count = 0; // FN
        public int MyProperty
        {
            get { return myVar; }
            set
            {
                count >>>= 42; // FN
                myVar = value;
            }
        }

        private int myVar;
    }

}
