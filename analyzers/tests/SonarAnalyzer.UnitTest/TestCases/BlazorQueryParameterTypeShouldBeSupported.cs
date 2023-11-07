using System;
using Microsoft.AspNetCore.Components;

[Route("/query-parameters")]
public class BlazorQueryParameterTypeShouldBeSupported : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public TimeSpan TimeSpan { get; set; } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}
    //     ^^^^^^^^

    [Parameter]
    public TimeSpan TimeSpanParam { get; set; } // Compliant
}
