/// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7624
class PrimaryConstructor1(object options)
{
    private readonly object _options = options;      // Compliant
    public object Options { get; } = options;        // Compliant
    public object[] AllOptions { get; } = [options]; // Compliant
}

class PrimaryConstructor2(object options)
{
    public PrimaryConstructor2() : this(null)
    {

    }
    private readonly object _options = options; // Compliant
}

class PrimaryConstructor3(object options)
{
    public PrimaryConstructor3() : this(null)
    {
        _options = null;
    }
    private readonly object _options = options; // Compliant
}

class CollectionExpression1
{
    private readonly int[] numbers = [1, 2, 3]; // Noncompliant
    //                             ^^^^^^^^^^^

    CollectionExpression1()
    {
        numbers = [4, 5, 6];
    }
}
