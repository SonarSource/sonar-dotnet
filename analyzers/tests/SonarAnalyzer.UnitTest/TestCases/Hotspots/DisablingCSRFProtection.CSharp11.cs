using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;

namespace Net6Poc.DisablingCSRFProtection
{
    internal class TestCases
    {
        [GenericIgnoreAntiforgeryToken<int>] // Noncompliant
        public void A() { }

        [NonGenericAttribute] // Noncompliant
        public void B() { }

        public void C() { }
    }
    public class NonGenericAttribute : IgnoreAntiforgeryTokenAttribute { }

    public class GenericIgnoreAntiforgeryToken<T> : IgnoreAntiforgeryTokenAttribute { }
}
