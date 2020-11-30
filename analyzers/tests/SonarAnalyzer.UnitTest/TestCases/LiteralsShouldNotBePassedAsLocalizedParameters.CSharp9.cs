using System.ComponentModel;

TopLevelLocalFunction("literal", "literal"); //Noncompliant
                                             //Noncompliant@-1

void TopLevelLocalFunction([Localizable(true)] string param1, string message)
{
}

record Record
{
    [Localizable(true)]
    public string Property1 { get; set; }
    public string Property2 { get; set; }

    public void Method()
    {
        Property1 = "lorem"; // Noncompliant
        Property2 = "ipsum";

        LocalFunctionWithAttribute("literal"); //Noncompliant

        void LocalFunctionWithAttribute([Localizable(true)] string param1)
        {
        }
    }
}
