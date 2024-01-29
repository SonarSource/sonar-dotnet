public class Sample
{
    private int @default;

    public void Go()
    {
        var x = @default;
        @default = 42;
    }
}
