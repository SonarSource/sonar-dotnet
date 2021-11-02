public class Sample
{
    public const string Prefix = "Prefix_";
    public const string Suffix = "_Suffix";
    public const string Name = $"{Prefix}Name{Suffix}";

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
        var character = 'c';

        (y, var z) =
            ("a",
             'x');
    }
}
