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
    public class DistributionTest
    {
        [TestMethod]
        public void Distribution()
        {
            var distribution = new Distribution(0, 10, 20);
            distribution.Ranges.Should().BeEquivalentTo(0, 10, 20);
            distribution.Values.Should().BeEquivalentTo(0, 0, 0);
            distribution.ToString().Should().BeEquivalentTo("0=0;10=0;20=0");

            distribution.Add(0);
            distribution.Values.Should().BeEquivalentTo(1, 0, 0);
            distribution.ToString().Should().BeEquivalentTo("0=1;10=0;20=0");

            distribution.Add(9);
            distribution.Values.Should().BeEquivalentTo(2, 0, 0);
            distribution.ToString().Should().BeEquivalentTo("0=2;10=0;20=0");

            distribution.Add(12);
            distribution.Values.Should().BeEquivalentTo(2, 1, 0);
            distribution.ToString().Should().BeEquivalentTo("0=2;10=1;20=0");

            distribution.Add(3);
            distribution.Values.Should().BeEquivalentTo(3, 1, 0);
            distribution.ToString().Should().BeEquivalentTo("0=3;10=1;20=0");

            distribution.Add(10);
            distribution.Values.Should().BeEquivalentTo(3, 2, 0);
            distribution.ToString().Should().BeEquivalentTo("0=3;10=2;20=0");

            distribution.Add(99);
            distribution.Values.Should().BeEquivalentTo(3, 2, 1);
            distribution.ToString().Should().BeEquivalentTo("0=3;10=2;20=1");

            distribution = new Distribution(7, 13);
            distribution.Ranges.Should().BeEquivalentTo(7, 13);
            distribution.Values.Should().BeEquivalentTo(0, 0);
            distribution.ToString().Should().BeEquivalentTo("7=0;13=0");

            distribution.Add(5);
            distribution.Values.Should().BeEquivalentTo(1, 0);
            distribution.ToString().Should().BeEquivalentTo("7=1;13=0");
        }
    }
}
