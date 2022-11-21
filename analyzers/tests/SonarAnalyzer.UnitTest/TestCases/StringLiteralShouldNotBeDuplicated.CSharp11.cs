using System.Diagnostics;

public class FooNonCompliant
{
    private string NameOne = """foobar"""; // Noncompliant {{Define a constant instead of using this literal '""foobar""' 4 times.}}

    private string NameTwo = """foobar"""; // Secondary

    public const string NameConst = """foobar"""; // Secondary

    public static readonly string NameReadonly = """foobar"""; // Secondary

}

public class FooLessThanFiveCharacters
{
    private string NameOne = """foo"""; // Compliant (less than 5 characters)

    private string NameTwo = """foo""";

    public const string NameConst = """foo""";

    public static readonly string NameReadonly = """foo""";
}

public class FooNonCompliantStringInterpolation
{
    static string GetName() => "Bar";

    public string NameOne = $"{
        GetName()
        }"; // FN - {{Define a constant instead of using this literal '""Bar""' 4 times.}}

    public string NameTwo = $"{
        GetName()
        }"; // FN

    public static string NameThree = "Bar"; // FN

    public static readonly string NameReadonly = $"{GetName()}"; // FN

}
