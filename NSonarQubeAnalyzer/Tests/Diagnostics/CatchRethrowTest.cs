using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    [TestClass]
    public class CatchRethrowTest
    {
        [TestMethod]
        public void CatchRethrow()
        {
            Verifier.Verify(@"TestCases\CatchRethrow.cs", new CatchRethrow());
        }
    }
}
