using Microsoft.AspNetCore.Components;

namespace RazorClassLib.S6800;

public partial class S6800_Partial
{
    [Parameter]
    public string Unmatched { get; set; }

    [Parameter]
    public DateTime Matched { get; set; }
}
