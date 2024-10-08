using System;
using Microsoft.AspNetCore.Components;

namespace Razor
{
    public partial class NotAssignedPrivateMember
    {
        private ElementReference pageTitle; // Compliant - Assigned in the generated code for the razor file.

        public void Test()
        {
            Console.WriteLine(pageTitle.ToString());
        }
    }
}
