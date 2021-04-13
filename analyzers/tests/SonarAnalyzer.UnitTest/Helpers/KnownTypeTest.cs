using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.UnitTest.Helpers
{
    [TestClass]
    public class KnownTypeTest
    {
        [TestMethod]
        public void ShortNameWithoutAttributeSuffix_ForAttribute_ShouldBeAttribute() =>
            KnownType.System_Attribute.ShortNameWithoutAttributeSuffix.Should().Be("Attribute");

        [TestMethod]
        public void ShortNameWithoutAttributeSuffix_ForObsoleteAttribute_ShouldBeObsolete() =>
            KnownType.System_ObsoleteAttribute.ShortNameWithoutAttributeSuffix.Should().Be("Obsolete");
    }
}
