namespace Tests.Diagnostics
{
    class Tests
    {
        public void Message()
        {
            var s1 = "Test";                              // Noncompliant {{Add the 'const' modifier to 's1', and replace 'var' with 'string'.}}
            string s2 = $"This is a {nameof(Message)}";   // Compliant - constant string interpolation is only valid in C# 10 and above
            var s3 = $"This is a {nameof(Message)}";      // Compliant - constant string interpolation is only valid in C# 10 and above
            var s4 = "This is a" + $" {nameof(Message)}"; // Compliant - constant string interpolation is only valid in C# 10 and above
            var s5 = $@"This is a {nameof(Message)}";     // Compliant - constant string interpolation is only valid in C# 10 and above
        }
    }
}
