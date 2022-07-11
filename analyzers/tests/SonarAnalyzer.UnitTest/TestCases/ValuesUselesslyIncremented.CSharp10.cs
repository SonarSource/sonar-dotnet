public struct S
{
    public S()
    {
        int i = 0;

        (i, var j) = (i++, 0); // Noncompliant {{Remove this increment or correct the code not to waste it.}}
        //            ^^^
        (var k, _) = (i++, 0); // Compliant
        (_, _) = (i++, 0);     // Compliant
    }

    public (int, int) M1(int i, int j)
    {
        return (i, j++);       // Noncompliant {{Remove this increment or correct the code not to waste it.}}
        //         ^^^
    }

    public (int, (int, int)) M2()
    {
        var (i, j, k) = (0, 0, 0);
        return (i++, (j++, k++));
        //      ^^^                 {{Remove this increment or correct the code not to waste it.}}
        //            ^^^       @-1 {{Remove this increment or correct the code not to waste it.}}
        //                 ^^^  @-2 {{Remove this increment or correct the code not to waste it.}}
    }

    public (int, (int, int)) M3(int i, int j, int k) =>
        (i++, (j++, k++));      // Noncompliant [lambdaTuple1, lambdaTuple2, lambdaTuple3]

    public (int, (int, int)) M4(int i, int j, int k) =>
        (i++, M1(j++, k++));    // Noncompliant
    //   ^^^
}
