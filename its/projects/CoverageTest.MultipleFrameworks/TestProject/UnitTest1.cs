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
            Assert.AreEqual("True", Sample.MatchingBranchpoints_FullyCovered(true));
#endif
            Assert.AreEqual("No", Sample.MatchingBranchpoints_FullyCovered(false));
#if NET
            Assert.AreEqual("True", Sample.NotMatchingBranchpoints_FullyCovered(true));
#endif
            Assert.AreEqual("No", Sample.NotMatchingBranchpoints_FullyCovered(false));
        }

        [TestMethod]
        public void PartiallyCovered()
        {
            Assert.AreEqual("True", Sample.MatchingBranchpoints_PartiallyCovered(true));
            Assert.AreEqual("True", Sample.NotMatchingBranchpoints_PartiallyCovered(true));
        }
    }
}
