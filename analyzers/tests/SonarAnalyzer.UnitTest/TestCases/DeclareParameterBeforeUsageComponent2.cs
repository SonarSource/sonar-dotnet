using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

public class DeclareParameterBeforeUsageComponent2 : DeclareParameterBeforeUsageComponent
{
    [Parameter]
    public string SecondMessage { get; set; }
}
