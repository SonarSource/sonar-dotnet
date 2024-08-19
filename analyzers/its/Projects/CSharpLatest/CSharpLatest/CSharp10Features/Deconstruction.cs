using System.ComponentModel;

namespace CSharpLatest.CSharp10Features;

public class Sonar
{
    private string? sonar;
    [Localizable(true)]
    public string? Source { get; set; }
    public void SomeTest1()
    {
        (var sonar, double _, Source) = ("Sonar", 24 / 7, "Source");
    }
}
