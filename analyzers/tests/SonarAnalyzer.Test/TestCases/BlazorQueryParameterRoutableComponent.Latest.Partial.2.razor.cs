using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject
{
    public partial class BlazorQueryParameterRoutableComponent_Latest_Partial
    {
        [Parameter]
        public partial TimeSpan MixedAttributesInPartials { get => default; set { } } // Noncompliant {{Query parameter type 'TimeSpan' is not supported.}}
    }
}
