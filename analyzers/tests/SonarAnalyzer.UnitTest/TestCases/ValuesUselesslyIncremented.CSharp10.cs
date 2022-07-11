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
        int i = 0; int j = 0; int k = 0;
        return (i++, (j++, k++));
        //      ^^^                 {{Remove this increment or correct the code not to waste it.}}
        //            ^^^       @-1 {{Remove this increment or correct the code not to waste it.}}
        //                 ^^^  @-2 {{Remove this increment or correct the code not to waste it.}}
    }
}
