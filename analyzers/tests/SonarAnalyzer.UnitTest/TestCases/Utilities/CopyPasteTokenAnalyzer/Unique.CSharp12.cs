class PrimaryConstructor(string a = "a", char b = 'b', /* Comment */ double c = 1)
{
    void Method()
    {
        var lambdaWithDefaultValues = (string x = "y",
                                       char y = 'z', // Comment
                                       double z = 1) => x;
        string[] collectionExpressionStr = ["Hello",
                                            "World"];
        char[] collectionExpressionChar = ['a',
                                           'b'];
        double[] collectionExpressionDouble = [1, // Comment
                                               2];
    }
}
