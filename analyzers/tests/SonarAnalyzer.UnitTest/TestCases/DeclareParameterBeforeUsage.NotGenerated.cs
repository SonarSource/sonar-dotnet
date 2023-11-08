using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

class DeclareParameterBeforeUsage : ComponentBase
{
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<global::EmptyProject.DeclareParameterBeforeUsage_PrintUnmatchedFalse>(0);
        builder.AddAttribute(1, "BoolParam");
        builder.AddAttribute(2, "Other", "1"); // Noncompliant
//      ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
        builder.CloseComponent();
    }
}
