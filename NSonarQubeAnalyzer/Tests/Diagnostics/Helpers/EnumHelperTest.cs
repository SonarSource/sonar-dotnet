
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer.Diagnostics.Helpers;
using NSonarQubeAnalyzer.Diagnostics.SonarProperties.Sqale;

namespace Tests.Diagnostics.Helpers
{
    [TestClass]
    public class EnumHelperTest
    {
        [TestMethod]
        public void ToSonarQubeString()
        {
            SqaleSubCharacteristic.ApiAbuse.ToSonarQubeString().Should().BeEquivalentTo("API_ABUSE");
        }
    }
}
