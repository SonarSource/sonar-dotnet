using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

public partial class ParameterTypeShouldMatchRouteTypeConstraint : ComponentBase
{
    [Parameter]
    public string BoolParam { get; set; } // Noncompliant [bool] {{Parameter type 'string' does not match route parameter type constraint.}}
}
