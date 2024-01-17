using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class TestAttributes
    {
        [Xunit.Fact]
        public async Task Xunit_Fact() { }

        [Xunit.Theory]
        public async Task Xunit_Theory() { }
    }
}
