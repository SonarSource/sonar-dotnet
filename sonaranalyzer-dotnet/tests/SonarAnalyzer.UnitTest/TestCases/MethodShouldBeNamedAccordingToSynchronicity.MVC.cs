using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace Tests.Diagnostics
{
    public class MyController : Controller
    {
        public async Task<ActionResult> Users() { return null; } // Compliant

        public async Task<object> Orders() { return null; } // Compliant

        public async Task<int> GetNumber() { return 5; } // Compliant
    }

    public class OtherController : MyController
    {
        public async Task<ActionResult> Emails() { return null; } // Compliant

        public async Task<object> Purchases() { return null; } // Compliant

        public async Task<int> GetAnotherNumber() { return 5; } // Compliant
    }
}
