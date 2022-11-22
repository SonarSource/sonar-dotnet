using System.ComponentModel;

class FooBar
{
    [Localizable(true)]
    public string Property1 { get; set; }

    public void Method()
    {
        string foo = "foo";
        string bar = "bar";

        Property1 = """
    This is a long message.
    It has several lines.
        Some are indented
                more than others.
    """; // Noncompliant @-5

        LocalFunctionWithAttribute("""
    This is a long message.
    It has several lines.
        Some are indented
                more than others..
    """); // Noncompliant @-5

        void LocalFunctionWithAttribute([Localizable(true)] string param1)
        {
        }
    }
}
