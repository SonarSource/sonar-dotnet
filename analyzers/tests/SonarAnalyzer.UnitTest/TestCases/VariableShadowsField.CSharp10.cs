public record struct S
{
    private int j = 0;

    public S()
    {
        (var i, var j) = (0, 0); // FN
    }
}
