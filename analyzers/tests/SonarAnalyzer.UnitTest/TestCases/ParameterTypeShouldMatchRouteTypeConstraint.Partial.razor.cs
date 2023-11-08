using Microsoft.AspNetCore.Components;

// Namespace in the test project
namespace EmptyProject
{
    public partial class ParameterTypeShouldMatchRouteTypeConstraint_Partial : ComponentBase
    {
        [Parameter]
        public string BoolParam { get; set; } // Noncompliant
    }
}
