
namespace Tests.Diagnostics
{

    ref struct X
    {
    }

    class NullableStructTest
    {
        public void TestMethod1(X x)
        {
            if (x == null) { return; }
        }

        public void TestMethod2(X nullableStruct)
        {
            if (nullableStruct != null) { return; }
        }

        public void TestMethod3(X nullableStruct)
        {
            nullableStruct = null;
            if (nullableStruct == null) // Noncompliant
            {
                return;
            }
        }

        public void TestMethod4(X nullableStruct)
        {
            nullableStruct = null;
            if (nullableStruct != null)  // Noncompliant
            { // Secondary
                return;
            }
        }

        public void TestMethod5(X nullableStruct)
        {
            nullableStruct = new X();
            if (nullableStruct == null) // Noncompliant
            { // Secondary
                return;
            }
        }

        public void TestMethod6(X nullableStruct)
        {
            nullableStruct = new X();
            if (nullableStruct != null)  // Noncompliant
            {
                return;
            }
        }
    }
}
