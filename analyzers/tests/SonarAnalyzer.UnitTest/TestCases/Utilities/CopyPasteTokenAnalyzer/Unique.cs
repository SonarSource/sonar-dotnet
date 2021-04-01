public class Sample
{
    public void Aaa()
    {
        var x = 42;
        var interpolatedWithWhitespaceToken = $"This literal should be $str but the whitespace between interpolation will not: {x} {x}";
        var interpolatedVerbatim = $@"Interpolated Verbatim";
        var verbatim = @"Verbatim";
    }

    public void Bbb()
    {
        var y = "value";
    }
}
