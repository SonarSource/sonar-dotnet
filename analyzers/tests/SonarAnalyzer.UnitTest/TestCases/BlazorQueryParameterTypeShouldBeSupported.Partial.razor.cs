using Microsoft.AspNetCore.Components;

namespace EmptyProject
{
    public partial class BlazorQueryParameterTypeShouldBeSupported_Partial : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public TimeSpan TimeSpan { get; set; } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}
        //     ^^^^^^^^

        [Parameter]
        public TimeSpan TimeSpanParam { get; set; } // Compliant
    }
}
