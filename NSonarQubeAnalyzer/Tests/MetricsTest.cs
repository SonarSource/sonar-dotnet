using System;
using System.Collections.Immutable;
using FluentAssertions;
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
            MetricsFor("").Lines().Should().Be(1);
            MetricsFor("\n").Lines().Should().Be(2);
            MetricsFor("\r").Lines().Should().Be(2);
            MetricsFor("\r\n").Lines().Should().Be(2);
            MetricsFor("\n").Lines().Should().Be(2);
            MetricsFor("using System;\r\n/*hello\r\nworld*/").Lines().Should().Be(3);
        }

        [TestMethod]
        public void Comments()
        {
            MetricsFor("").Comments().Should().BeEmpty();
            MetricsFor("#ifdef DEBUG\nfoo\n#endif").Comments().Should().BeEmpty();
            MetricsFor("// l1").Comments().Should().BeEquivalentTo(1);
            MetricsFor("// l1\n// l2").Comments().Should().BeEquivalentTo(1, 2);
            MetricsFor("/* l1 */").Comments().Should().BeEquivalentTo(1);
            MetricsFor("/* l1 \n l2 */").Comments().Should().BeEquivalentTo(1, 2);
            MetricsFor("/* l1 \n l2 */").Comments().Should().BeEquivalentTo(1, 2);
            MetricsFor("/// foo").Comments().Should().BeEquivalentTo(1);
            MetricsFor("/** */").Comments().Should().BeEquivalentTo(1);
        }

        private static Metrics MetricsFor(string text)
        {
            return new Metrics(CSharpSyntaxTree.ParseText(text));
        }
    }
}
