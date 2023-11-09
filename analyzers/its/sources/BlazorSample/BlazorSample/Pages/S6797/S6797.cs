using System;
using Microsoft.AspNetCore.Components;

[Route("/query-parameters")]
public class S6797 : ComponentBase
{
    [Parameter]
    [SupplyParameterFromQuery]
    public TimeSpan TimeSpan { get; set; }

    [Parameter]
    public TimeSpan TimeSpanParam { get; set; }
}
