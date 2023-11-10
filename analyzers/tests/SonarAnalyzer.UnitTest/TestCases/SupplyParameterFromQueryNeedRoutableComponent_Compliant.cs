using Microsoft.AspNetCore.Components;
using System;

[Route("/query-parameters")]
public class SupplyParameterFromQueryNeedRoutableComponent_Compliant : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public TimeSpan TimeSpan { get; set; } // Compliant

    [Parameter]
    public string SupplyParameterFromQueryAttributeMissing { get; set; } // Compliant: missing [SupplyParameterFromQuery]

    [SupplyParameterFromQuery]
    public string ParameterAttributeMissing { get; set; } // Compliant: missing [Parameter]
}
