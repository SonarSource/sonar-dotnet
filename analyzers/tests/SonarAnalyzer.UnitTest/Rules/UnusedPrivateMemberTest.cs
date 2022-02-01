/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

using System.Collections.Immutable;
using System.Linq;
using FluentAssertions;
using FluentAssertions.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Rules.CSharp;
using SonarAnalyzer.UnitTest.MetadataReferences;
using SonarAnalyzer.UnitTest.TestFramework;

namespace SonarAnalyzer.UnitTest.Rules
{
    [TestClass]
    public partial class UnusedPrivateMemberTest
    {
        private readonly VerifierBuilder builder = new VerifierBuilder<UnusedPrivateMember>();

        [TestMethod]
        public void UnusedPrivateMember_DebuggerDisplay_Attribute() =>
            builder.AddSnippet(@"
// https://github.com/SonarSource/sonar-dotnet/issues/1195
[System.Diagnostics.DebuggerDisplay(""{field1}"", Name = ""{Property1} {Property3}"", Type = ""{Method1()}"")]
public class MethodUsages
{
    private int field1;
    private int field2; // Noncompliant
    private int Property1 { get; set; }
    private int Property2 { get; set; } // Noncompliant
    private int Property3 { get; set; }
    private int Method1() { return 0; }
    private int Method2() { return 0; } // Noncompliant

    public void Method()
    {
        var x = Property3;
    }
}").Verify();

        [TestMethod]
        public void UnusedPrivateMember_Members_With_Attributes_Are_Not_Removable() =>
            builder.AddSnippet(@"
using System;
public class FieldUsages
{
    [Obsolete]
    private int field1;

    [Obsolete]
    private int Property1 { get; set; }

    [Obsolete]
    private int Method1() { return 0; }

    [Obsolete]
    private class Class1 { }
}").Verify();

        [TestMethod]
        public void UnusedPrivateMember_Assembly_Level_Attributes() =>
            builder.AddSnippet(@"
[assembly: System.Reflection.AssemblyCompany(Foo.Constants.AppCompany)]
public static class Foo
{
    internal static class Constants // Compliant, detect usages from assembly level attributes.
    {
        public const string AppCompany = ""foo"";
    }
}").Verify();

        [TestMethod]
        public void UnusedPrivateMemberWithPartialClasses() =>
            builder.AddPaths("UnusedPrivateMember.part1.cs", "UnusedPrivateMember.part2.cs").Verify();

        [TestMethod]
        public void UnusedPrivateMember_Methods_EventHandler() =>
            // Event handler methods are not reported because in WPF an event handler
            // could be added through XAML and no warning will be generated if the
            // method is removed, which could lead to serious problems that are hard
            // to diagnose.
            builder.AddSnippet(@"
using System;
public class NewClass
{
    private void Handler(object sender, EventArgs e) { } // Noncompliant
}
public partial class PartialClass
{
    private void Handler(object sender, EventArgs e) { } // intentional False Negative
}").Verify();

        [TestMethod]
        public void UnusedPrivateMember_Unity3D_Ignored() =>
            builder.AddSnippet(@"
// https://github.com/SonarSource/sonar-dotnet/issues/159
public class UnityMessages1 : UnityEngine.MonoBehaviour
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages2 : UnityEngine.ScriptableObject
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages3 : UnityEditor.AssetPostprocessor
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

public class UnityMessages4 : UnityEditor.AssetModificationProcessor
{
    private void SomeMethod(bool hasFocus) { } // Compliant
}

// Unity3D does not seem to be available as a nuget package and we cannot use the original classes
namespace UnityEngine
{
    public class MonoBehaviour { }
    public class ScriptableObject { }
}
namespace UnityEditor
{
    public class AssetPostprocessor { }
    public class AssetModificationProcessor { }
}").Verify();

        [TestMethod]
        public void EntityFrameworkMigration_Ignored() =>
            builder.AddSnippet(@"
namespace EntityFrameworkMigrations
{
    using Microsoft.EntityFrameworkCore.Migrations;

    public class SkipMigration : Migration
    {
        private void SomeMethod(bool condition) { } // Compliant

        protected override void Up(MigrationBuilder migrationBuilder) { }
    }
}").AddReferences(EntityFrameworkCoreReferences(Constants.NuGetLatestVersion)).Verify();

        [DataTestMethod]
        [DataRow(ProjectType.Product)]
        [DataRow(ProjectType.Test)]
        public void UnusedPrivateMember(ProjectType projectType) =>
            builder.AddPaths("UnusedPrivateMember.cs").AddReferences(TestHelper.ProjectTypeReference(projectType)).Verify();

        [TestMethod]
        public void UnusedPrivateMember_FromCSharp7() =>
            builder.AddPaths("UnusedPrivateMember.CSharp7.cs").WithOptions(ParseOptionsHelper.FromCSharp7).Verify();

        [TestMethod]
        public void UnusedPrivateMember_FromCSharp8() =>
            builder.AddPaths("UnusedPrivateMember.CSharp8.cs")
                .WithOptions(ParseOptionsHelper.FromCSharp8)
                .AddReferences(MetadataReferenceFacade.NETStandard21)
                .AddReferences(MetadataReferenceFacade.MicrosoftExtensionsDependencyInjectionAbstractions)
                .Verify();

#if NET

        [TestMethod]
        public void UnusedPrivateMember_FromCSharp9() =>
            builder.AddPaths("UnusedPrivateMember.CSharp9.cs", "UnusedPrivateMember.CSharp9.Second.cs").WithOptions(ParseOptionsHelper.FromCSharp9).Verify();

        [TestMethod]
        public void UnusedPrivateMember_FromCSharp9_TopLevelStatements() =>
            builder.AddPaths("UnusedPrivateMember.CSharp9.TopLevelStatements.cs").WithTopLevelStatements().Verify();

        [TestMethod]
        public void UnusedPrivateMember_FromCSharp10() =>
            builder.AddPaths("UnusedPrivateMember.CSharp10.cs").WithOptions(ParseOptionsHelper.FromCSharp10).Verify();

        [TestMethod]
        public void UnusedPrivateMember_FromCSharpPreview() =>
            builder.AddPaths("UnusedPrivateMember.CSharpPreview.cs").WithOptions(ParseOptionsHelper.CSharpPreview).Verify();

#endif

        [TestMethod]
        public void UnusedPrivateMember_CodeFix() =>
            builder.AddPaths("UnusedPrivateMember.cs")
                .WithCodeFix<UnusedPrivateMemberCodeFix>()
                .WithCodeFixedPaths("UnusedPrivateMember.Fixed.cs", "UnusedPrivateMember.Fixed.Batch.cs")
                .VerifyCodeFix();

        [TestMethod]
        public void UnusedPrivateMember_UsedInGeneratedFile() =>
            builder.AddPaths("UnusedPrivateMember.CalledFromGenerated.cs", "UnusedPrivateMember.Generated.cs").Verify();

        [TestMethod]
        public void UnusedPrivateMember_Performance() =>
            // Once the NuGet packages are downloaded, the time to execute the analyzer on the given file is
            // about ~1 sec. It was reduced from ~11 min by skipping Guids when processing ObjectCreationExpression.
            // The threshold is set here to 30 seconds to avoid flaky builds due to slow build agents or network connections.
            builder.AddPaths("UnusedPrivateMember.Performance.cs")
                .AddReferences(EntityFrameworkCoreReferences("5.0.12"))   // The latest before 6.0.0 for .NET 6 that has Linq versioning collision issue
                .Invoking(x => x.Verify())
                .ExecutionTime().Should().BeLessOrEqualTo(30.Seconds());

        private static ImmutableArray<MetadataReference> EntityFrameworkCoreReferences(string entityFrameworkVersion) =>
            NetStandardMetadataReference.Netstandard
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(entityFrameworkVersion))
                .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(entityFrameworkVersion))
                .ToImmutableArray();
    }
}
