using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.Web;

namespace Razor
{
    public partial class NotAssignedPrivateMember
    {
        private PageTitle pageTitle; // Noncompliant

        public void Test()
        {
            Console.WriteLine(pageTitle.ToString());
        }
    }
}
