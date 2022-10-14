public class Sample
{
    private const string StringLiteralToken = "StringLiteralToken";
    private const int NumericToken = 42;
    private string interpolatedWithWhitespaceTokenInside = $"{NumericToken} {NumericToken}";
    public const string SingleLineRawString = """"SingleLine"""";
    public const string MultiLineRawString =
        """"
        MultiLine
        """";
    public string InterpolatedSingleLineRawString = $""""SingleLine{NumericToken}"""";
    public string InterpolatedMultiLineRawString =
        $$""""
        MultiLine{{NumericToken}}
        """";

    public void A()
    {
        var UTF8String = "UTF8 string"u8;
        var UTF8SingleLineRawString = """"SingleLine""""u8;
        var UTF8MultiLineRawString =
            """"
        MultiLine
        """"u8;
    }
}
