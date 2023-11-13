using Microsoft.AspNetCore.Components;

namespace RazorClassLib.S6800;

[Route("/S6800/csharp-only/{Unmatched:bool}")]
[Route("/S6800/csharp-only/{Matched:datetime}")]
[Route(Constants.ROUTE_UNMATCHED)]
[Route(Constants.ROUTE_MATCHED)]
public class S6800_CsharpOnly
{
    [Parameter]
    public string Unmatched { get; set; }

    [Parameter]
    public DateTime Matched { get; set; }

    [Parameter]
    public string UnmatchedFromConstant { get; set; }

    [Parameter]
    public string MatchedFromConstant { get; set; }
}
