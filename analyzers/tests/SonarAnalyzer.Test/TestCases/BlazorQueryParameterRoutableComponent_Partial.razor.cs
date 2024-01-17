using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject
{
    public partial class BlazorQueryParameterRoutableComponent_Partial : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Number { get; set; } // Compliant

        // S6803 Compliant: the .razor part of the component defines the route
        [Parameter]
        [SupplyParameterFromQuery]
        public TimeSpan TimeSpan { get; set; } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}
        //     ^^^^^^^^

        [Parameter]
        public TimeSpan TimeSpanParam { get; set; } // Compliant
    }
}
