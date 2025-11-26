using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject
{
    public class Nullable { }

    class BlazorQueryParameterRoutableComponent_Noncompliant : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Number { get; set; } // Noncompliant {{Component parameters can only receive query parameter values in routable components.}}
        //         ^^^^^^

        [Parameter]
        [SupplyParameterFromQuery]
        public string MyString { get; set; } // Noncompliant
    }

    [Route("/my-route")]
    class BlazorQueryParameterRoutableComponent_Noncompliant_S6797 : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public Nullable Nullable { get; set; } // Noncompliant {{Query parameter type 'Nullable' is not supported.}}
    }
}

