using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class TestAttributes
    {
        [NUnit.Framework.Test]
        public async Task NUnit_Test() { }

        [NUnit.Framework.TestCase(1)]
        public async Task NUnit_TestCase(int i) { }

        [NUnit.Framework.TestCaseSource("foo")]
        public async Task NUnit_TestCaseSource() { }

        [NUnit.Framework.Theory]
        public async Task NUnit_Theory() { }
    }
}
