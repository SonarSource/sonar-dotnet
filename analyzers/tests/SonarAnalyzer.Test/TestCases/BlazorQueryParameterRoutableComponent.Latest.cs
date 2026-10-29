using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject
{
    [Route("/my-route")]
    class BlazorQueryParameterRoutableComponent_DateAndTime : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public DateOnly Date { get; set; } // Noncompliant {{Query parameter type 'DateOnly' is not supported.}} FP NET-4887

        [Parameter]
        [SupplyParameterFromQuery]
        public TimeOnly? Time { get; set; } // Noncompliant {{Query parameter type 'TimeOnly' is not supported.}} FP NET-4887
    }
}
