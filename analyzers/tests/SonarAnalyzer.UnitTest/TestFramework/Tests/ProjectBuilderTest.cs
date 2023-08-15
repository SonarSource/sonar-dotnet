namespace SonarAnalyzer.UnitTest.TestFramework.Tests;

[TestClass]
public class ProjectBuilderTest
{
    private static readonly ProjectBuilder EmptyCS = SolutionBuilder.Create().AddProject(AnalyzerLanguage.CSharp);
    private static readonly ProjectBuilder EmptyVB = SolutionBuilder.Create().AddProject(AnalyzerLanguage.VisualBasic);

    [TestMethod]
    public void AddDocument_ValidExtension()
    {
        EmptyCS.AddDocument("TestCases\\VariableUnused.cs").FindDocument("VariableUnused.cs").Should().NotBeNull();
        EmptyVB.AddDocument("TestCases\\VariableUnused.vb").FindDocument("VariableUnused.vb").Should().NotBeNull();
    }

    [TestMethod]
    public void AddDocument_MismatchingExtension()
    {
        Action f;

        f = () => EmptyCS.AddDocument("TestCases\\VariableUnused.vb");
        f.Should().Throw<ArgumentException>();

        f = () => EmptyVB.AddDocument("TestCases\\VariableUnused.cs");
        f.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void AddDocument_InvalidExtension()
    {
        Action f;

        f = () => EmptyCS.AddDocument("TestCases\\VariableUnused.unknown");
        f.Should().Throw<ArgumentException>();

        f = () => EmptyVB.AddDocument("TestCases\\VariableUnused.unknown");
        f.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void AddDocument_SupportsRazorFiles()
    {
        EmptyCS.AddDocument("TestCases\\UnusedPrivateMember.razor").FindDocument("UnusedPrivateMember.razor").Should().NotBeNull();
        EmptyVB.AddDocument("TestCases\\UnusedPrivateMember.razor").FindDocument("UnusedPrivateMember.razor").Should().NotBeNull();
    }

    [TestMethod]
    public void AddDocument_CsharpSupportsCshtmlFiles() =>
        EmptyCS.AddDocument("TestCases\\UnusedPrivateMember.cshtml").FindDocument("UnusedPrivateMember.cshtml").Should().NotBeNull();

    [TestMethod]
    public void AddDocument_VbnetDoesntSupportCshtmlFiles() =>
        new Action(() => EmptyVB.AddDocument("TestCases\\UnusedPrivateMember.cshtml").FindDocument("UnusedPrivateMember.cshtml").Should().NotBeNull())
            .Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddDocument_CsharpDoesntSupportVbnetFiles() =>
        new Action(() => EmptyCS.AddDocument("TestCases\\UnusedPrivateMember.vbhtml").FindDocument("UnusedPrivateMember.vbhtml").Should().NotBeNull())
            .Should().Throw<ArgumentException>();

    [TestMethod]
    public void AddDocument_VbnetDoesntSupportVbnetFiles() =>
        new Action(() => EmptyVB.AddDocument("TestCases\\UnusedPrivateMember.vbhtml").FindDocument("UnusedPrivateMember.vbhtml").Should().NotBeNull())
            .Should().Throw<ArgumentException>();
}
