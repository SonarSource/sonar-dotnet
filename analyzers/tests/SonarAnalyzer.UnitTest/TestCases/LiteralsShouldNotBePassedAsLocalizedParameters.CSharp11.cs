using System.ComponentModel;

class FooBar
{
    [Localizable(true)]
    public string Property1 { get; set; }

    public void Method()
    {
        Property1 = """
            This is a long message.
            It has several lines.
                Some are indented
                        more than others.
            """; // Noncompliant @-5

        LocalFunction("""
            This is a long message.
            It has several lines.
                Some are indented
                        more than others..
            """); // Noncompliant @-5

        void LocalFunction([Localizable(true)] string param1) { }
    }
}
