using Microsoft.AspNetCore.Components;

namespace EmptyProject
{
    public partial class ParameterTypeShouldMatchRouteTypeConstraint_Partial : ComponentBase
    {
        [Parameter]
        public string BoolParam { get; set; } // Noncompliant
    }
}
