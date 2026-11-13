using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject
{
    [Route("/my-route")]
    class BlazorQueryParameterRoutableComponent_DateAndTime : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public DateOnly Date { get; set; } // Compliant NET-4887 DateOnly is bound from the query string

        [Parameter]
        [SupplyParameterFromQuery]
        public TimeOnly? Time { get; set; } // Compliant NET-4887 TimeOnly? is bound from the query string

        [Parameter]
        [SupplyParameterFromQuery]
        public TimeOnly TimeValue { get; set; } // Compliant

        [Parameter]
        [SupplyParameterFromQuery]
        public DateOnly? NullableDate { get; set; } // Compliant

        [Parameter]
        [SupplyParameterFromQuery]
        public DateOnly[] Dates { get; set; } // Compliant

        [Parameter]
        [SupplyParameterFromQuery]
        public TimeOnly?[] Times { get; set; } // Compliant

        [Parameter]
        [SupplyParameterFromQuery]
        public DateTimeOffset Offset { get; set; } // Noncompliant {{Query parameter type 'DateTimeOffset' is not supported.}}

        [Parameter]
        [SupplyParameterFromQuery]
        public DateTimeOffset? NullableOffset { get; set; } // Noncompliant {{Query parameter type 'DateTimeOffset' is not supported.}}
    }
}
