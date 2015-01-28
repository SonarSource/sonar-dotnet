using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSonarQubeAnalyzer;
using FluentAssertions;
using System.Collections.Immutable;

namespace Tests
{
    [TestClass]
    public class ConfigurationTest
    {
        [TestMethod]
        public void Configuration()
        {
            Configuration conf = new Configuration(XDocument.Load("ConfigurationTest.xml"));
            conf.IgnoreHeaderComments.Should().BeTrue();
            conf.Files.Should().BeEquivalentTo(@"C:\MyClass1.cs", @"C:\MyClass2.cs");

            conf.AnalyzerIds.Should().BeEquivalentTo(
                "AssignmentInsideSubExpression",
                "AsyncAwaitIdentifier",
                "BreakOutsideSwitch",

                "FileLoc",
                "FunctionComplexity",
                "LineLength",
                "S1479",
                "S1067",
                "S107",
                "MagicNumber",
                "S101",
                "S100",
                "S124");

            var analyzers = conf.Analyzers();
            analyzers.OfType<FileLines>().Single().Maximum.ShouldBeEquivalentTo(1000);
            analyzers.OfType<LineLength>().Single().Maximum.ShouldBeEquivalentTo(200);
            analyzers.OfType<TooManyLabelsInSwitch>().Single().Maximum.ShouldBeEquivalentTo(30);
            analyzers.OfType<TooManyParameters>().Single().Maximum.ShouldBeEquivalentTo(7);
            analyzers.OfType<ExpressionComplexity>().Single().Maximum.ShouldBeEquivalentTo(3);
            analyzers.OfType<FunctionComplexity>().Single().Maximum.ShouldBeEquivalentTo(10);
            analyzers.OfType<ClassName>().Single().Convention.ShouldBeEquivalentTo("^(?:[A-HJ-Z][a-zA-Z0-9]+|I[a-z0-9][a-zA-Z0-9]*)$");
            analyzers.OfType<MethodName>().Single().Convention.ShouldBeEquivalentTo("^[A-Z][a-zA-Z0-9]+$");
            analyzers.OfType<MagicNumber>().Single().Exceptions.ShouldBeEquivalentTo(ImmutableHashSet.Create("0", "1", "0x0", "0x00", ".0", ".1", "0.0", "1.0"));

            var commentAnalyzer = analyzers.OfType<CommentRegularExpression>().Single();
            commentAnalyzer.Rules.Should().HaveCount(2);
            commentAnalyzer.Rules[0].Descriptor.Id.ShouldBeEquivalentTo("TODO");
            commentAnalyzer.Rules[0].Descriptor.MessageFormat.ShouldBeEquivalentTo("Fix this TODO");
            commentAnalyzer.Rules[0].RegularExpression.ShouldBeEquivalentTo(".*TODO.*");
            commentAnalyzer.Rules[1].Descriptor.Id.ShouldBeEquivalentTo("FIXME");
            commentAnalyzer.Rules[1].Descriptor.MessageFormat.ShouldBeEquivalentTo("Fix this FIXME");
            commentAnalyzer.Rules[1].RegularExpression.ShouldBeEquivalentTo(".*FIXME.*");
        }
    }
}
