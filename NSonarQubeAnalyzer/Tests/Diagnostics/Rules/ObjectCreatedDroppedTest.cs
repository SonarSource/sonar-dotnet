using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Rules;

namespace Tests.Diagnostics.Rules
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
