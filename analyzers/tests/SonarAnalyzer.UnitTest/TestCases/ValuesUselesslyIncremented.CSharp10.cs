public struct S
{
    public S()
    {
        int i = 0;

        (i, var j) = (i++, 0); // FN
    }

    public (int, int) M(int i, int j)
    {
        return (i, j++); // FN
    }
}
