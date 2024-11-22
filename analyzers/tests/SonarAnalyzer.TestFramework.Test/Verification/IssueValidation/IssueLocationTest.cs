/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Test.Verification.IssueValidation;

[TestClass]
public class IssueLocationTest
{
    private static readonly IssueLocation Issue = new(IssueType.Primary, "File.cs", 42, "Lorem ipsum", null, 42, 10, "S1234");

    [TestMethod]
    public void IssueLocation_Ctor_NoSecondaryMessage_HasEmptyMessage() =>
        new IssueLocation(Issue, new SecondaryLocation(Location.None, null)).Message.Should().BeEmpty();

    [TestMethod]
    public void IssueLocation_Ctor_Primary()
    {
        var sut = new IssueLocation(Diagnostic.Create(AnalysisScaffolding.CreateDescriptor("Sxxxx"), null));
        sut.Type.Should().Be(IssueType.Primary);
        sut.RuleId.Should().Be("Sxxxx");
        sut.IssueId.Should().BeNull();
    }

    [TestMethod]
    public void IssueLocation_Ctor_Error()
    {
        var sut = new IssueLocation(Diagnostic.Create("CSxxxx", "Category", "Message", DiagnosticSeverity.Error, DiagnosticSeverity.Error, true, 0));
        sut.Type.Should().Be(IssueType.Error);
        sut.RuleId.Should().Be("CSxxxx");
        sut.IssueId.Should().Be("CSxxxx");
    }

    [TestMethod]
    public void IssueLocation_Ctor_SecondaryLocation()
    {
        var sut = new IssueLocation(Issue, new SecondaryLocation(Location.None, null));
        sut.Type.Should().Be(IssueType.Secondary);
        sut.RuleId.Should().Be("S1234");
    }

    [TestMethod]
    public void IssueLocation_Ctor_NoLocation_HasEmptyFilePath()
    {
        var sut = new IssueLocation(Diagnostic.Create(AnalysisScaffolding.CreateDescriptor("Sxxxx"), null));
        sut.FilePath.Should().BeEmpty();
        sut.LineNumber.Should().Be(1);
        sut.Start.Should().Be(0);
    }

    [TestMethod]
    public void IssueLocation_GetHashCode_DependsOnlyOnKey()
    {
        var hashcode = new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx").GetHashCode();
        hashcode.Should().Be(new IssueLocation(IssueType.Primary, "File.cs", 1, "Diff", "diff", 99, 99, "Sdiff").GetHashCode());
        hashcode.Should().NotBe(0);
        hashcode.Should().NotBe(new IssueLocation(IssueType.Primary, "Diff.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx").GetHashCode());
        hashcode.Should().NotBe(new IssueLocation(IssueType.Primary, "File.cs", 9, "Msg1", "id1", 2, 4, "Sxxxx").GetHashCode());
        hashcode.Should().NotBe(new IssueLocation(IssueType.Secondary, "File.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx").GetHashCode());
    }

    [TestMethod]
    public void IssueLocation_Equals_FullMatch()
    {
        var orig = new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx");
        var same = new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx");
        var proj = new IssueLocation(IssueType.Primary, string.Empty, 1, "Msg1", "id1", 2, 4, "Sxxxx");
        orig.Equals(same).Should().BeTrue();
        orig.Equals(proj).Should().BeTrue();
        proj.Equals(same).Should().BeTrue();
        orig.Equals(null).Should().BeFalse();
        orig.Equals("different type").Should().BeFalse();
        orig.Equals(new IssueLocation(IssueType.Secondary, "File.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx")).Should().BeFalse();
        orig.Equals(new IssueLocation(IssueType.Primary, "Diff.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx")).Should().BeFalse();
        orig.Equals(new IssueLocation(IssueType.Primary, "File.cs", 9, "Msg1", "id1", 2, 4, "Sxxxx")).Should().BeFalse();
        orig.Equals(new IssueLocation(IssueType.Primary, "File.cs", 1, "Xxxx", "id1", 2, 4, "Sxxxx")).Should().BeFalse();
        orig.Equals(new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "id1", 9, 4, "Sxxxx")).Should().BeFalse();
        orig.Equals(new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "id1", 2, 9, "Sxxxx")).Should().BeFalse();
        orig.Equals(new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "id1", 2, 4, "Syyyy")).Should().BeFalse();
        // Special case, IssueId is not checked for IsPrimary issues
        orig.Equals(new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "xxxx", 2, 4, "Sxxxx")).Should().BeTrue();
        orig.Equals(new IssueLocation(IssueType.Secondary, "File.cs", 1, "Msg1", "xxxx", 2, 4, "Sxxxx")).Should().BeFalse();
    }

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_RuleId()
    {
        var hasValue = new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx");
        var withNull = new IssueLocation(IssueType.Primary, "File.cs", 1, "Msg1", "id1", 2, 4, "Sxxxx");
        hasValue.Equals(withNull).Should().BeTrue();
        withNull.Equals(hasValue).Should().BeTrue();
        withNull.Equals(withNull).Should().BeTrue();
        hasValue.Equals(hasValue).Should().BeTrue();
    }

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_Message() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation(IssueType.Primary, "File.cs", 1, "M1", "id1", 2, 4, "Sxxxx"),
            new IssueLocation(IssueType.Primary, "File.cs", 1, null, "id1", 2, 4, "Sxxxx"));

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_IssueId() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation(IssueType.Primary, "File.cs", 1, "M1", "i1", 2, 4, "Sxxxx"),
            new IssueLocation(IssueType.Primary, "File.cs", 1, "M1", null, 2, 4, "Sxxxx"));

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_Start() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation(IssueType.Primary, "File.cs", 1, "M1", "id1", 2, 4, "Sxxxx"),
            new IssueLocation(IssueType.Primary, "File.cs", 1, "M1", "id1", null, 4, "Sxxxx"));

    [TestMethod]
    public void IssueLocation__Equals_IgnoreNull_Length() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation(IssueType.Primary, "File.cs", 1, "M1", "id1", 2, 4, "Sxxxx"),
            new IssueLocation(IssueType.Primary, "File.cs", 1, "M1", "id1", 2, null, "Sxxxx"));

    [TestMethod]
    public void IssueLocationKey_CreatedFromIssue()
    {
        var sut = new IssueLocationKey(Issue);
        sut.FilePath.Should().Be("File.cs");
        sut.LineNumber.Should().Be(42);
        sut.Type.Should().Be(IssueType.Primary);
    }

    [TestMethod]
    public void IssueLocationKey_IsMatch_MatchesSameKey() =>
        new IssueLocationKey(IssueType.Primary, "File.cs", 42).IsMatch(Issue).Should().BeTrue();

    [TestMethod]
    public void IssueLocationKey_IsMatch_ProjectLevelMatchesAnyPath() =>
        new IssueLocationKey(IssueType.Primary, string.Empty, 42).IsMatch(Issue).Should().BeTrue();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentFilePath() =>
        new IssueLocationKey(IssueType.Primary, "Another.cs", 42).IsMatch(Issue).Should().BeFalse();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentLineNumber() =>
        new IssueLocationKey(IssueType.Primary, "File.cs", 1024).IsMatch(Issue).Should().BeFalse();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentIsPrimary() =>
        new IssueLocationKey(IssueType.Secondary, "File.cs", 42).IsMatch(Issue).Should().BeFalse();

    private static void ValidateEqualsIgnoreNull(IssueLocation hasValue, IssueLocation withNull)
    {
        hasValue.Equals(withNull).Should().BeTrue();
        withNull.Equals(hasValue).Should().BeTrue();
        withNull.Equals(withNull).Should().BeTrue();
        hasValue.Equals(hasValue).Should().BeTrue();
    }
}
