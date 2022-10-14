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
