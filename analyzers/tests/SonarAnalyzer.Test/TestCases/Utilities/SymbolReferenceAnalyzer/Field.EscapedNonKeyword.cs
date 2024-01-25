public class Sample
{
    private int @nonkeyword;

    public void Go()
    {
        var x = nonkeyword;
        @nonkeyword = 42;
    }
}
