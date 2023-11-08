using Microsoft.AspNetCore.Components;

namespace BlazorSample.Pages.S6800;

public partial class S6800Partial
{
    [Parameter]
    public string Unmatched { get; set; }

    [Parameter]
    public DateTime Matched { get; set; }
}
