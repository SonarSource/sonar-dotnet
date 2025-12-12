using Point = (int, int);

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

class PrimaryConstructor(System.String p1, string p2 = "default value that should be tokenized as string" /* a comment */, int p3 = 1)
{
    void Method()
    {
        var lambdaWithDefaultValues = (string l1 = "default value that should be tokenized as string", int l2 = 2) => l1;
        var usingAliasDirective = new Point(0, 0);
        string[] collectionExpression = ["Hello", "World"];
    }
}

class SubClass() : PrimaryConstructor("something")
{
    public SubClass(int p1) : this() { }
}

static class Extensions
{
    extension(string s)
    {
        public int Length => s.Length;
    }
}

public class FieldKeyword
{
    public int Value
    {
        get { return field; }
        set { field = value; }
    }
}
