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
    private static readonly IssueLocation Issue = new() { RuleId = "S1234", FilePath = "File.cs", LineNumber = 42, IsPrimary = true, Message = "Lorem ipsum", Start = 42, Length = 10 };

    [TestMethod]
    public void IssueLocation_Ctor_NoSecondaryMessage_HasEmptyMessage() =>
        new IssueLocation(new SecondaryLocation(Location.None, null)).Message.Should().BeEmpty();

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
        var hashcode = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }.GetHashCode();
        hashcode.Should().Be(new IssueLocation { RuleId = "Sdiff", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Diff", IssueId = "diff", Start = 99, Length = 99 }.GetHashCode());
        hashcode.Should().NotBe(0);
        hashcode.Should().NotBe(new IssueLocation { RuleId = "Sxxx", FilePath = "Diff.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }.GetHashCode());
        hashcode.Should().NotBe(new IssueLocation { RuleId = "Sxxx", FilePath = "File.cs", LineNumber = 9, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }.GetHashCode());
        hashcode.Should().NotBe(new IssueLocation { RuleId = "Sxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = false, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }.GetHashCode());
    }

    [TestMethod]
    public void IssueLocation_Equals_FullMatch()
    {
        var orig = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        var same = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        orig.Equals(same).Should().BeTrue();
        orig.Equals(null).Should().BeFalse();
        orig.Equals("different type").Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Syyyy", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "Diff.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 9, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = false, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Xxxx", IssueId = "id1", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "xxx", Start = 2, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 9, Length = 4 }).Should().BeFalse();
        orig.Equals(new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 9 }).Should().BeFalse();
    }

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_RuleId()
    {
        var hasValue = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        var withNull = new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "Msg1", IssueId = "id1", Start = 2, Length = 4 };
        hasValue.Equals(withNull).Should().BeTrue();
        withNull.Equals(hasValue).Should().BeTrue();
        withNull.Equals(withNull).Should().BeTrue();
        hasValue.Equals(hasValue).Should().BeTrue();
    }

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_Message() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "M1", IssueId = "id1", Start = 2, Length = 4 },
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = null, IssueId = "id1", Start = 2, Length = 4 });

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_IssueId() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "M1", IssueId = "i1", Start = 2, Length = 4 },
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "M1", IssueId = null, Start = 2, Length = 4 });

    [TestMethod]
    public void IssueLocation_Equals_IgnoreNull_Start() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "M1", IssueId = "id1", Start = 2, Length = 4 },
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "M1", IssueId = "id1", Start = null, Length = 4 });

    [TestMethod]
    public void IssueLocation__Equals_IgnoreNull_Length() =>
        ValidateEqualsIgnoreNull(
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "M1", IssueId = "id1", Start = 2, Length = 4 },
            new IssueLocation { RuleId = "Sxxxx", FilePath = "File.cs", LineNumber = 1, IsPrimary = true, Message = "M1", IssueId = "id1", Start = 2, Length = null });

    [TestMethod]
    public void IssueLocationKey_CreatedFromIssue()
    {
        var sut = new IssueLocationKey(Issue);
        sut.FilePath.Should().Be("File.cs");
        sut.LineNumber.Should().Be(42);
        sut.IsPrimary.Should().BeTrue();
    }

    [TestMethod]
    public void IssueLocationKey_IsMatch_MatchesSameKey() =>
        new IssueLocationKey("File.cs", 42, true).IsMatch(Issue).Should().BeTrue();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentFilePath() =>
        new IssueLocationKey("Another.cs", 42, true).IsMatch(Issue).Should().BeFalse();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentLineNumber() =>
        new IssueLocationKey("File.cs", 1024, true).IsMatch(Issue).Should().BeFalse();

    [TestMethod]
    public void IssueLocationKey_IsMatch_DifferentIsPrimary() =>
        new IssueLocationKey("File.cs", 42, false).IsMatch(Issue).Should().BeFalse();

    private static void ValidateEqualsIgnoreNull(IssueLocation hasValue, IssueLocation withNull)
    {
        hasValue.Equals(withNull).Should().BeTrue();
        withNull.Equals(hasValue).Should().BeTrue();
        withNull.Equals(withNull).Should().BeTrue();
        hasValue.Equals(hasValue).Should().BeTrue();
    }
}
