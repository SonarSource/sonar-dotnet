using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

[Route("/route/{BoolParam:bool}")] // Secondary [bool] {{This route parameter has a 'bool' type constraint.}}
//             ^^^^^^^^^^^^^^^^
public partial class ParameterTypeShouldMatchRouteTypeConstraint : ComponentBase
{
}
