using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Net6Poc.DisablingCSRFProtection
{
    internal class TestCases
    {
        [GenericIgnoreAntiforgeryToken<int>]    // FN - for performance reasons inheritance is not supported
        public void A() { }

        [IgnoreAntiforgeryTokenAttribute]       // Noncompliant
        public void B() { }
    }

    public class GenericIgnoreAntiforgeryToken<T> : IgnoreAntiforgeryTokenAttribute { }
}
