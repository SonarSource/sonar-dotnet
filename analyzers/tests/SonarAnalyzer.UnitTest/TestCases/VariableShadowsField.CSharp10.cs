public record struct S
{
    private int i = 0;
    private int j = 0;
    private int k = 0;
    private int l = 0;
    private int n = 0;
    private int p = 0;
    private int q = 0;
    private int r = 0;
    private int s = 0;
    private int t = 0;
    private int u = 0;
    private int v = 0;
    private int w = 0;

    public S()
    {
        (var i, var j) = (0, 0);
        //   ^
        //          ^ @-1

        var (k, l) = (0, 0);     // Noncompliant [issue1,issue2]
        (var m, var n) = (0, 0); // Noncompliant m is not declared as field
        //          ^
        var (o, p) = (0, 0);     // Noncompliant
        (var a, var b) = (0, 0); // Compliant
        var (c, d) = (0, 0);     // Compliant
        var (_, _) = (0, 0);     // Compliant

        var (q, (_, r, _), s) = (1, (2, 3, 4), 5);
        //   ^
        //          ^        @-1
        //                 ^ @-2
        foreach ((var t, var u) in new[] { (1, 2) })
        //            ^
        //                   ^ @-1
        {

        }
        foreach (var (v, w) in new[] { (1, 2) })
        //            ^
        //               ^ @-1
        {

        }
    }
}
