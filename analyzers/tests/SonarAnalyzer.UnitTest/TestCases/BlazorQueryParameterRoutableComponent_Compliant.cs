using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject
{
    [Route("/query-parameters")]
    class BlazorQueryParameterRoutableComponent_Compliant : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Number { get; set; } // Compliant

        [Parameter, SupplyParameterFromQuery]
        public int MyInt { get; set; } // Compliant

        [Parameter]
        public TimeSpan SupplyParameterFromQueryAttributeMissing { get; set; } // Compliant: missing [SupplyParameterFromQuery]

        [SupplyParameterFromQuery]
        public TimeSpan ParameterAttributeMissing { get; set; } // Compliant: missing [Parameter]
    }
}
