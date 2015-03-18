using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics;

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
