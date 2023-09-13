using Microsoft.AspNetCore.Components;

namespace TestSamples;

public partial class Sample
{
    [Parameter]
    public Dictionary<string, object> Attributes { get; set; } = new (); // Noncompliant, FP
}
