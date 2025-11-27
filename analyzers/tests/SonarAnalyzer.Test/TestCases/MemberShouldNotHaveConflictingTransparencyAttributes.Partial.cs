using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;

namespace Tests.Diagnostics
{
    public partial class PartialClass
    {
        [SecuritySafeCritical]  // FN https://sonarsource.atlassian.net/browse/NET-2717
        private void Method() { }
    }
}
