public class TestCases
{
    public void SameBaseType(bool condition)
    {
        object x;
        X o = null;
        if (o == null) // Compliant, target type conditionals are supported only from C# 9
        {
            x = new Y();
        }
        else
        {
            x = o;
        }

        o = o ?? new X(); // Noncompliant {{Use the '??=' operator here.}}

        Base elem;
        if (condition) // Compliant, target type conditionals are supported only from C# 9
        {
            elem = new A();
        }
        else
        {
            elem = new B();
        }
    }

    class X { }
    class Y { }
    class Base { }
    class A : Base { }
    class B : Base { }
}

// https://github.com/SonarSource/sonar-dotnet/issues/4607
public class Example
{
    public string Foo { get; set; }
    public string Bar { get; set; }

    public Example Fallback(Example other)
    {
        return new Example
        {
            Foo = Foo ?? other.Foo, // Compliant, cannot be changed
            Bar = Bar ?? other.Bar  // Compliant, cannot be changed
        };
    }
}

// https://community.sonarsource.com/t/134362
class Repro_134362
{
    public void Method(bool condition1, bool condition2)
    {
        string s1 = string.Empty;
        if (condition1) // Compliant, we don't raise if one of the if branches contains ternary
                        // Otherwise the fix, raises S3358
        {
            s1 = "some value";
        }
        else
        {
            s1 = $"other value {(condition2 ? "suffix1" : "suffix2")}";
        }
    }

    public void Method2(bool condition1, bool condition2)
    {
        string s1 = string.Empty;
        if (condition1) // Compliant, we don't raise if one of the if branches contains ternary
                        // Otherwise the fix, raises S3358
        {
            s1 = "some value";
        }
        else
        {
            s1 = condition2 ? "suffix1" : "suffix2";
        }
    }
}
