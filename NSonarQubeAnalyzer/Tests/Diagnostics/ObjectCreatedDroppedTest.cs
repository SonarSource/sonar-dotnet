using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Diagnostics
{
    [TestClass]
    public class ObjectCreatedDroppedTest
    {
        [TestMethod]
        public void ObjectCreatedDropped()
        {
            Verifier.Verify(@"TestCases\ObjectCreatedDropped.cs", new ObjectCreatedDropped());
        }
    }
}
