using Point = (int, int);

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
