using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class S3874 : I3874
    {
        public static void SetRef(ref I3874 obj) // compliant because this is interface implementation
        {
            obj = new S3874();
        }

        public static void SetOut(out I3874 obj) // compliant because this is interface implementation
        {
            obj = new S3874();
        }
    }

    public interface I3874
    {
        static abstract void SetRef(ref I3874 obj); // Noncompliant
        static abstract void SetOut(out I3874 obj); // Noncompliant
    }
}
