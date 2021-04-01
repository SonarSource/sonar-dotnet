public class Sample
{
    public void Go()
    {
        var x = LocalMethod();

        int LocalMethod() => 42;
    }
}
