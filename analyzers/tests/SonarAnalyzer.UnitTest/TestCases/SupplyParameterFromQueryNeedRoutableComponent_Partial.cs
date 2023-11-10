using Microsoft.AspNetCore.Components;
using System;

namespace EmptyProject
{
    public partial class SupplyParameterFromQueryNeedRoutableComponent_Partial : ComponentBase
    {
        [Parameter]
        [SupplyParameterFromQuery]
        public int Number { get; set; } // Compliant
    }
}
