using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Net6Poc.DisablingCSRFProtection
{
    internal class TestCases
    {
        [GenericIgnoreAntiforgeryToken<int>] // FN
        public void A() { }

        [NonGenericAttribute] // FN
        public void B() { }

        public void C() { }
    }
    public class NonGenericAttribute : IgnoreAntiforgeryTokenAttribute { }

    public class GenericIgnoreAntiforgeryToken<T> : IgnoreAntiforgeryTokenAttribute { }
}
