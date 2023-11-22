/// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7624
class WithPrimaryConstructor(object options)
{
    private readonly object _options = options;      // Compliant
    public object Options { get; } = options;        // Compliant
    public object[] AllOptions { get; } = [options]; // Compliant
}

class WithReferencedPrimaryConstructor(object options)
{
    public WithReferencedPrimaryConstructor() : this(null)
    {

    }
    private readonly object _options = options; // Compliant
}

class WithPrimaryConstructorAndAssignment(object options)
{
    public WithPrimaryConstructorAndAssignment() : this(null)
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
