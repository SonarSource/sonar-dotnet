/// Reproducer for https://github.com/SonarSource/sonar-dotnet/issues/7624
class SampleClass1(object options)
{
    private readonly object _options = options; // Compliant
}

class SampleClass2(object options)
{
    public SampleClass2() : this(null)
    {

    }
    private readonly object _options = options; // Compliant
}
