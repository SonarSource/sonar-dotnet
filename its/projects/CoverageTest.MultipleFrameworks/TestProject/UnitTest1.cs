using ClassLibrary;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace TestProject
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void FullyCovered()
        {
#if NET
            Assert.AreEqual("True", MatchingBranchpoints_FullyCovered.Method(true));
#endif
            Assert.AreEqual("No", MatchingBranchpoints_FullyCovered.Method(false));
#if NET
            Assert.AreEqual("True", NotMatchingBranchpoints_FullyCovered.Method(true));
#endif
            Assert.AreEqual("No", NotMatchingBranchpoints_FullyCovered.Method(false));
        }

        [TestMethod]
        public void PartiallyCovered()
        {
            Assert.AreEqual("True", MatchingBranchpoints_PartiallyCovered.Method(true));
            Assert.AreEqual("True", NotMatchingBranchpoints_PartiallyCovered.Method(true));
        }
    }
}
