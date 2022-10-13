public class Sample
{
    public const string Prefix = "Prefix_";
    public const string Suffix = "_Suffix";
    public const string Name = $"{Prefix}Name{Suffix}";
    public const string SingleLineRawString = """"SingleLine"""";
    public const string MultiLineRawString =
        """"
        MultiLine
        """";
    public const string InterpolatedSingleLineRawString = $""""SingleLine{Prefix}"""";
    public const string InterpolatedMultiLineRawString =
        $$""""
        MultiLine{{Suffix}}
        """";

    public void Aaa()
    {
        var x = 42;
        var interpolatedWithWhitespace = $"This literal will be $str and there will be another $str between the interpolations: {x} {x}";
        var interpolatedVerbatim = $@"Interpolated Verbatim";
        var verbatim = @"Verbatim";
        var UTF8String = "UTF8 string"u8;
        var UTF8SingleLineRawString = """"SingleLine""""u8;
        var UTF8MultiLineRawString =
            """"
        MultiLine
        """"u8;
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
