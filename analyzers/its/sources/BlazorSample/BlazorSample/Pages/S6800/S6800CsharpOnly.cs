using Microsoft.AspNetCore.Components;

namespace BlazorSample.Pages.S6800;

[Route("/S6800/csharp-only/{Unmatched:bool}")]
[Route("/S6800/csharp-only/{Matched:datetime}")]
public class S6800CsharpOnly
{
    [Parameter]
    public string Unmatched { get; set; }

    [Parameter]
    public DateTime Matched { get; set; }
}
