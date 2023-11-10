using Microsoft.AspNetCore.Components;
using System;

class SupplyParameterFromQueryNeedsRoutableComponent_Noncompliant : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public int Number { get; set; } // Noncompliant {{Component parameters can only receive query parameter values in routable components.}}
    //         ^^^^^^

    [Parameter]
    [SupplyParameterFromQuery]
    public string MyString { get; set; } // Noncompliant
}
