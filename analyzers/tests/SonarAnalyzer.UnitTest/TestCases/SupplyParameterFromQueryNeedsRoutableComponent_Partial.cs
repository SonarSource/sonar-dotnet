using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject;

partial class SupplyParameterFromQueryNeedsRoutableComponent_Partial : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public TimeSpan TimeSpan { get; set; } // Compliant: the .razor part of the component defines the route
}
