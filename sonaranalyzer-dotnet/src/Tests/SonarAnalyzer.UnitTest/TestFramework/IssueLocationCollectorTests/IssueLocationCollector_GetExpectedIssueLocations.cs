using FluentAssertions;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace SonarAnalyzer.UnitTest.TestFramework.IssueLocationCollectorTests
{
    [TestClass]
    public class IssueLocationCollector_GetExpectedIssueLocations
    {
        [TestMethod]
        public void GetExpectedIssueLocations_No_Comments()
        {
            var code = @"public class Foo
{
    public void Bar(object o)
    {
        Console.WriteLine(o);
    }
}";
            var locations = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().BeEmpty();
        }

        [TestMethod]
        public void GetExpectedIssueLocations_Locations()
        {
            var code = @"public class Foo
{
    public void Bar(object o) // Noncompliant
    {
        // Noncompliant @+1
        Console.WriteLine(o);
    }
}";
            var locations = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(2);

            locations.Select(l => l.IsPrimary).Should().Equal(new[] { true, true });
            locations.Select(l => l.LineNumber).Should().Equal(new[] { 3, 5 });
        }

        [TestMethod]
        public void GetExpectedIssueLocations_ExactLocations()
        {
            var code = @"public class Foo
{
    public void Bar(object o)
//              ^^^
//                         ^ Secondary@-1
    {
        Console.WriteLine(o);
    }
}";
            var locations = new IssueLocationCollector().GetExpectedIssueLocations(SourceText.From(code).Lines);

            locations.Should().HaveCount(2);

            locations.Select(l => l.IsPrimary).Should().BeEquivalentTo(new[] { true, false });
            locations.Select(l => l.LineNumber).Should().Equal(new[] { 3, 3 });
        }
    }
}
