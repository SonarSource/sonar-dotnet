using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

public class DeclareParameterBeforeUsageComponent : ComponentBase
{
    [Parameter]
    public string Message { get; set; }
}
