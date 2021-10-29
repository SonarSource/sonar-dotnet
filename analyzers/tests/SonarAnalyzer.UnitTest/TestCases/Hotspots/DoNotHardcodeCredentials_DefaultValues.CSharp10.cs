namespace Tests.Diagnostics
{
    class Program
    {
        public void Test()
        {
            const string part1 = "pass";
            const string part2 = "word";
            string noncompliant = $"{part1}{part2}"; // FN
            string compliant = $"{part2}{part1}";
        }
    }
}
