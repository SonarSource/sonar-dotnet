using System;
using Microsoft.AspNetCore.Components;

public class Nullable
{

}

[Route("/query-parameters")]
public class BlazorQueryParameterTypeShouldBeSupported : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public TimeSpan TimeSpan { get; set; } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}
    //     ^^^^^^^^

    [Parameter]
    public TimeSpan TimeSpanParam { get; set; } // Compliant

    [Parameter]
    [SupplyParameterFromQuery]
    public Nullable Nullable { get; set; } // Noncompliant {{Query parameter type 'Nullable' is not supported.}}

}
