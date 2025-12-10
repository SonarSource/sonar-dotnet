using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

[Route("""/route/{RawStringLiteralParam:bool}""")]
[Route("""/route/{RawStringLiteralParam:int}""")] // Secondary [raw-int]
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^
[Route($$"""/route/{RawInterpolatedParam:{{Constants.BoolConstraint}}}""")]
[Route($$"""/route/{RawInterpolatedParam:{{Constants.IntConstraint}}}""")] // Secondary [raw-interpolated-int]
//     ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
[Route("/something" + "/{ConcatenationParam:bool}")]
[Route("/something" + "/{ConcatenationParam:int}")] // Secondary [concatenation-int]
//                      ^^^^^^^^^^^^^^^^^^^^^^^^
public class ParameterTypeShouldMatchRouteTypeConstraint_Constant : ComponentBase
{
    [Parameter]
    public bool RawStringLiteralParam { get; set; } // Noncompliant [raw-int]
    [Parameter]
    public bool RawInterpolatedParam { get; set; } // Noncompliant [raw-interpolated-int]
}
