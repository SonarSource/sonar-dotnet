using Microsoft.AspNetCore.Components;

namespace RazorClassLib.S6797;

[Route("/query-parameters")]
public class S6797_CsharpOnly : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public TimeSpan TimeSpan { get; set; } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}
    //     ^^^^^^^^

    [Parameter]
    public TimeSpan TimeSpanParam { get; set; } // Compliant
}
