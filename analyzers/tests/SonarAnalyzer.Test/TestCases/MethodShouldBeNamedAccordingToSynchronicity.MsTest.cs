using System;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    public class TestAttributes
    {
        [Microsoft.VisualStudio.TestTools.UnitTesting.TestMethod]
        public async Task MSTest_TestMethod() { }

        [Microsoft.VisualStudio.TestTools.UnitTesting.DataTestMethod]
        public async Task MSTest_DataTestMethod() { }

        [DerivedTestMethodAttribute]
        public async Task MSTest_DerivedTestMethod() { }
    }

    public class DerivedTestMethodAttribute : Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute
    {

    }
}
