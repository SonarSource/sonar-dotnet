using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;

namespace Tests
{
    [TestClass]
    public class MetricsTest
    {
        [TestMethod]
        public void Lines()
        {
            Assert.AreEqual(1, MetricsFor("").Lines());
            Assert.AreEqual(2, MetricsFor("\n").Lines());
            Assert.AreEqual(2, MetricsFor("\r").Lines());
            Assert.AreEqual(2, MetricsFor("\r\n").Lines());
            Assert.AreEqual(2, MetricsFor("\n").Lines());
            Assert.AreEqual(3, MetricsFor("using System;\r\n/*hello\r\nworld*/").Lines());
        }

        private static Metrics MetricsFor(string text)
        {
            return new Metrics(CSharpSyntaxTree.ParseText(text));
        }
    }
}
