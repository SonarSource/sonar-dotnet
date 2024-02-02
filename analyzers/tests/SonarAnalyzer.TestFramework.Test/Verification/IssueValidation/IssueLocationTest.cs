/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
 * mailto: contact AT sonarsource DOT com
 *
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the GNU Lesser General Public
 * License as published by the Free Software Foundation; either
 * version 3 of the License, or (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
 * Lesser General Public License for more details.
 *
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program; if not, write to the Free Software Foundation,
 * Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using SonarAnalyzer.Test;
using SonarAnalyzer.TestFramework.Verification.IssueValidation;

namespace SonarAnalyzer.TestFramework.Test.Verification.IssueValidation;

[TestClass]
public class IssueLocationTest
{
    private static readonly IssueLocation Issue = new() { RuleId = "S1234", FilePath = "File.cs", LineNumber = 42, Type = IssueType.Primary, Message = "Lorem ipsum", Start = 42, Length = 10 };

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
        var hashcode = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }.GetHashCode();
        hashcode.Should().Be(new IssueLocation { RuleId = "Sdiff", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Diff", IssueId = "diff", Start = 99, Length = 99 }.GetHashCode());
        hashcode.Should().NotBe(0);
        hashcode.Should().NotBe(new IssueLocation { RuleId = "Sxxx", FilePath = "Diff.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }.GetHashCode());
        hashcode.Should().NotBe(new IssueLocation { RuleId = "Sxxx", FilePath = "File.cs", LineNumber = 9, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }.GetHashCode());
        hashcode.Should().NotBe(new IssueLocation { RuleId = "Sxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Secondary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }.GetHashCode());
    }

    [TestMethod]
    public void IssueLocation_Equals_FullMatch()
    {
        var orig = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        var same = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        var proj = new IssueLocation { RuleId = "Sxxxx", FilePath = string.Empty, LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        orig.Equals(same).Should().BeTrue();
        orig.Equals(proj).Should().BeTrue();
        proj.Equals(same).Should().BeTrue();
        orig.Equals(null).Should().BeFalse();
        orig.Equals("different type").Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Syyyy", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "Diff.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 9, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Secondary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Xxxx", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 9, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 9 }).Should().BeFalse();
        // Special case, IssueId is not checked for IsPrimary issues
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "xxxx", Start = 2, Length = 4 }).Should().BeTrue();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Secondary, Message = "Msg1", IssueId = "xxx", Start = 2, Length = 4 }).Should().BeFalse();
    }

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_RuleId()
    {
        var hasValue = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        var withNull = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        hasValue.Equals(withNull).Should().BeTrue();
        withNull.Equals(hasValue).Should().BeTrue();
        withNull.Equals(withNull).Should().BeTrue();
        hasValue.Equals(hasValue).Should().BeTrue();
    }

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_Message() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "M1", IssueId = "id1", Start = 2, Length = 4 },
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = null, IssueId = "id1", Start = 2, Length = 4 });

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_IssueId() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "M1", IssueId = "i1", Start = 2, Length = 4 },
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "M1", IssueId = null, Start = 2, Length = 4 });

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_Start() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "M1", IssueId = "id1", Start = 2, Length = 4 },
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "M1", IssueId = "id1", Start = null, Length = 4 });

    [TestMethod]
    public void IssueLocation__Equals_IgnoreNull_Length() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "M1", IssueId = "id1", Start = 2, Length = 4 },
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, Type = IssueType.Primary, Message = "M1", IssueId = "id1", Start = 2, Length = null });

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
        new IssueLocationKey("File.cs", 42, IssueType.Primary).IsMatch(Issue).Should().BeTrue();

    [TestMethod]
    public void IssueLocationKey_IsMatch_ProjectLevelMatchesAnyPath() =>
        new IssueLocationKey(string.Empty, 42, IssueType.Primary).IsMatch(Issue).Should().BeTrue();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentFilePath() =>
        new IssueLocationKey("Another.cs", 42, IssueType.Primary).IsMatch(Issue).Should().BeFalse();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentLineNumber() =>
        new IssueLocationKey("File.cs", 1024, IssueType.Primary).IsMatch(Issue).Should().BeFalse();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentIsPrimary() =>
        new IssueLocationKey("File.cs", 42, IssueType.Secondary).IsMatch(Issue).Should().BeFalse();

    private static void ValidateEqualsIgnoreNull(IssueLocation hasValue, IssueLocation withNull)
    {
        hasValue.Equals(withNull).Should().BeTrue();
        withNull.Equals(hasValue).Should().BeTrue();
        withNull.Equals(withNull).Should().BeTrue();
        hasValue.Equals(hasValue).Should().BeTrue();
    }
}
