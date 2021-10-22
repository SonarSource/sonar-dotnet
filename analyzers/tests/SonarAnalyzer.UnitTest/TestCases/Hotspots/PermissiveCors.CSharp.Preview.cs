using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Cors;

namespace Net6Poc.PermissiveCors
{
    internal class TestCases
    {
        [GenericAttribute<int>("*")] // Compliant - "*" is the policy name in this case
        public void A() { }

        [GenericAttribute<int>]
        public void B() { }

        [EnableCors()]
        public void C() { }

        [EnableCors("*")]
        public void D() { }
    }

    public class GenericAttribute<T> : EnableCorsAttribute
    {
        public GenericAttribute() : base() { }

        public GenericAttribute(string policyName) : base(policyName) { }
    }
}
