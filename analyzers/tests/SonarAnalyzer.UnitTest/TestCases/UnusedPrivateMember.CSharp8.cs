namespace Tests.Diagnostics
{
    public interface MyInterface1
    {
        public void Method1() { }
    }

    public class Class1
    {
        private interface MyInterface2 // Noncompliant
        {
            public void Method1() { }
        }
    }

    public class ReproIssue3842
    {
        public void SomeMethod()
        {
            ForSwitchArm x = null;
            var result = (x) switch
            {
                null => 1,
                // normally, when deconstructing in a switch, we don't actually know the type
                (object x1, object x2) => 2
            };
            var y = new ForIsPattern();
            if (y is (string a, string b)) { }
        }

        private sealed class ForSwitchArm
        {
            public void Deconstruct(out object a, out object b) { a = b = null; } // Noncompliant FP
        }

        private sealed class ForIsPattern
        {
            public void Deconstruct(out string a, out string b) { a = b = null; } // Noncompliant FP
        }
    }

}
