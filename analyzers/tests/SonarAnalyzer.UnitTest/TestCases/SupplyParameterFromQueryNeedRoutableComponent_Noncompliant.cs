using Microsoft.AspNetCore.Components;
using System;

public class SupplyParameterFromQueryNeedRoutableComponent_Noncompliant : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public TimeSpan TimeSpan { get; set; } // Noncompliant {{Component parameters can only receive query parameter values in routable components.}}
    //              ^^^^^^^^
}
