namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            const string part1 = "Password";
            const string part2 = "123";
            const string randomString = "RandomValue";
            const string noncompliant = $"{part1}={part2}"; // Noncompliant
            const string compliant = $"{randomString}={part2}";
        }
    }
}
