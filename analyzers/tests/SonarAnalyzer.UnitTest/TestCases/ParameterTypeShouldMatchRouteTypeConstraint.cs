using System;
using Microsoft.AspNetCore.Components;

[Route("/route/{BoolParam:bool}")] // Secondary [bool] {{This route parameter has a 'bool' type constraint.}}
//             ^^^^^^^^^^^^^^^^
[Route("/route/{DatetimeParam:datetime}")] // Secondary [datetime] {{This route parameter has a 'datetime' type constraint.}}
//             ^^^^^^^^^^^^^^^^^^^^^^^^
[Route("/route/{DatetimeParam:datetime}/{BoolParam:bool}")] // Secondary ^16#24 [datetime-multiple] {{This route parameter has a 'datetime' type constraint.}}
                                                            // Secondary@-1 ^41#16 [bool-multiple] {{This route parameter has a 'bool' type constraint.}}
[Route("/route/{CompliantBoolParam:bool}")]
[Route("/route/{CompliantDatetimeParam:datetime}")]
public class ParameterTypeShouldMatchRouteTypeConstraint : ComponentBase
{
    [Parameter]
    public string BoolParam { get; set; } // Noncompliant [bool] {{Parameter type 'string' does not match route parameter type constraint.}}
                                          // Noncompliant@-1 [bool-multiple] {{Parameter type 'string' does not match route parameter type constraint.}}
    [Parameter]
    public decimal DatetimeParam { get; set; } // Noncompliant [datetime] {{Parameter type 'decimal' does not match route parameter type constraint.}}
                                               // Noncompliant@-1 [datetime-multiple] {{Parameter type 'decimal' does not match route parameter type constraint.}}
    [Parameter]
    public bool CompliantBoolParam { get; set; } // Compliant
    [Parameter]
    public DateTime CompliantDatetimeParam { get; set; } // Compliant
}
