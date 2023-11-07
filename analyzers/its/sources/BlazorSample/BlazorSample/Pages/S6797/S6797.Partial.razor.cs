using Microsoft.AspNetCore.Components;

namespace BlazorSample.Pages.S6797
{
    public partial class S6797_Partial : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public TimeSpan TimeSpan { get; set; } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}
        //     ^^^^^^^^

        [Parameter]
        public TimeSpan TimeSpanParam { get; set; } // Compliant
    }
}
