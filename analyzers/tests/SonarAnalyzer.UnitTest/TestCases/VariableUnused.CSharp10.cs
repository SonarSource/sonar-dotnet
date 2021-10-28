public record struct S
{
    private int j = 0;

    public S()
    {
        (var i, // FN
         var j) = (0, 0); // FN

        var a = 0;
        (a, var b) = (0, 0); // FN
    }
}
