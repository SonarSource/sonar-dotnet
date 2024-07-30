using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;
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
