record Record
{
    public void Method()
    {
        const string Part1 = "Part1";
        const string Part2 = "Part2";
        const string MergedString = $"{Part1} and {Part2}";
        var a = "fee fie foe " + MergedString.ToString(); // Noncompliant {{There's no need to call 'ToString()' on a string.}}
    }
}
