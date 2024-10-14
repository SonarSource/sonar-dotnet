using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject
{
    public partial class BlazorQueryParameterRoutableComponent_Latest_Partial : ComponentBase
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

        public partial TimeSpan DefinedInBlazor { get => default; set { } } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}

        [Parameter]
        [SupplyParameterFromQuery]
        public partial TimeSpan ImplementedInBlazor { get; set; } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}

        [SupplyParameterFromQuery]
        public partial TimeSpan MixedAttributes { get => default; set { } } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}

        [SupplyParameterFromQuery]
        public partial TimeSpan MixedAttributesInPartials { get; set; } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}
    }
}
