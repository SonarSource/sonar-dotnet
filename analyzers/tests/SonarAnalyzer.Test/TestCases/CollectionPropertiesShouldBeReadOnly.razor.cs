using Microsoft.AspNetCore.Components;

namespace TestSamples
{
    public partial class Sample
    {
        [Parameter]
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>(); // Compliant

        public Dictionary<string, object> Items { get; set; } = new Dictionary<string, object>(); // Noncompliant
    }
}
