namespace CSharpLatest.CSharp11Features;

internal interface IGetNext<T> where T : IGetNext<T>
{
    static abstract T operator ++(T other);
}

internal struct RepeatSequence : IGetNext<RepeatSequence>
{
    private const char Ch = 'A';

    public string Text = new(Ch, 1);

    public RepeatSequence() { }

    public static RepeatSequence operator ++(RepeatSequence other)
        => other with { Text = other.Text + Ch };

    public override string ToString() => Text;
}

internal class Consumer
{
    public void Method()
    {
        var str = new RepeatSequence();

        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(str++);
        }
    }
}
