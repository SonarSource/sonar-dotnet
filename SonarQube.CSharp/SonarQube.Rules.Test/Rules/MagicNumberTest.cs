using System.Collections.Immutable;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Rules;

namespace SonarQube.Rules.Test.Rules
{
    [TestClass]
    public class MagicNumberTest
    {
        [TestMethod]
        public void MagicNumber()
        {
            var diagnostic = new MagicNumber
            {
                Exceptions = ImmutableHashSet.Create("0", "1", "0x0", "0x00", ".0", ".1", "0.0", "1.0")
            };
            Verifier.Verify(@"TestCases\MagicNumber.cs", diagnostic);
        }
    }
}
