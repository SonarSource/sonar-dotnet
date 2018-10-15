using System;

namespace Tests.Diagnostics
{
    class Foo
    {
        public void Test()
        {
            bool b = true;



            bool? x = false;
            NUnit.Framework.Assert.AreEqual(false, x); // Compliant, since the comparison triggers a conversion

        }
    }
}
