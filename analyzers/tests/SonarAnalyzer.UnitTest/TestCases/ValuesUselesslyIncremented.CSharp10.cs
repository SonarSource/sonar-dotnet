public struct S
{
    public S()
    {
        int i = 0;

        (i, var j) = (i++, 0); // FN
        (var k, _) = (i++, 0); // Compliant
        (_, _) = (i++, 0);     // Compliant
    }

    public (int, int) M(int i, int j)
    {
        return (i, j++); // FN
    }
}
