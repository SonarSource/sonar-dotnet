record Record
{
    public void Method()
    {
        var s = "foo";
        var a = "fee fie foe " + s.ToString(); // Noncompliant {{There's no need to call 'ToString()' on a string.}}
        var b = "test" + this.ToString(); // Noncompliant
        var c = "" + 1.ToString(); // Compliant, value type

        nint n = -5;
        var d = "" + n.ToString(); // Compliant, value type

        nuint nu = 5;
        var e = "" + nu.ToString(); // Compliant, value type

        PrintMembers(new());
    }
}
