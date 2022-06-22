public record struct S
{
    private int i = 0;
    private int j = 0;
    private int k = 0;
    private int l = 0;
    private int n = 0;
    private int p = 0;

    public S()
    {
        (var i, var j) = (0, 0); // Noncompliant
        // Noncompliant@-1

        var (k, l) = (0, 0); // Noncompliant
        // Noncompliant@-1

        (var m, var n) = (0, 0); // Noncompliant

        var (o, p) = (0, 0); // Noncompliant

        (var a, var b) = (0, 0); // Compliant

        var (c, d) = (0, 0); // Compliant
    }
}
