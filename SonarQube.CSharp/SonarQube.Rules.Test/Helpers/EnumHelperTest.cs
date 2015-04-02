using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarQube.Analyzers.Helpers;
using SonarQube.Analyzers.SonarQube.Settings.Sqale;

namespace SonarQube.Rules.Test.Helpers
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
