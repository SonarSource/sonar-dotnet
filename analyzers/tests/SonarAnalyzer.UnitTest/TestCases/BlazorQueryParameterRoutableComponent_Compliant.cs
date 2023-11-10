using Microsoft.AspNetCore.Components;
using System;

[Route("/query-parameters")]
public class BlazorQueryParameterRoutableComponent_Compliant : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public int Number { get; set; } // Compliant

    [Parameter, SupplyParameterFromQuery]
    public int MyInt { get; set; } // Compliant

    [Parameter]
    public string SupplyParameterFromQueryAttributeMissing { get; set; } // Compliant: missing [SupplyParameterFromQuery]

    [SupplyParameterFromQuery]
    public string ParameterAttributeMissing { get; set; } // Compliant: missing [Parameter]

    [Parameter]
    public TimeSpan TimeSpanParam { get; set; } // Compliant: missing [SupplyParameterFromQuery]
}
