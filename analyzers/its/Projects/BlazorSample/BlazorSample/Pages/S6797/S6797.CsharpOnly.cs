using System;
using Microsoft.AspNetCore.Components;

namespace BlazorSample.Pages.S6797
{
    [Route("/query-parameters")]
    public class S6797_CsharpOnly : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public TimeSpan TimeSpan { get; set; }

        [Parameter]
        public TimeSpan TimeSpanParam { get; set; }
    }
}
