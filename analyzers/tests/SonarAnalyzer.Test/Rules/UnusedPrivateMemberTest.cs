/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

using FluentAssertions.Extensions;
using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public partial class UnusedPrivateMemberTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<UnusedPrivateMember>();

    [TestMethod]
    public void UnusedPrivateMember_DebuggerDisplay_Attribute() =>
        builder.AddSnippet("""
            // https://github.com/SonarSource/sonar-dotnet/issues/1195
            [System.Diagnostics.DebuggerDisplay("{field1}", Name = "{Property1} {Property3}", Type = "{Method1()}")]
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
            }
            """).Verify();

    [TestMethod]
    public void UnusedPrivateMember_Members_With_Attributes_Are_Not_Removable() =>
        builder.AddSnippet("""
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
            }
            """).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_Assembly_Level_Attributes() =>
        builder.AddSnippet("""
            [assembly: System.Reflection.AssemblyCompany(Foo.Constants.AppCompany)]
            public static class Foo
            {
                internal static class Constants // Compliant, detect usages from assembly level attributes.
                {
                    public const string AppCompany = "foo";
                }
            }
            """).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMemberWithPartialClasses() =>
        builder.AddPaths("UnusedPrivateMember.part1.cs", "UnusedPrivateMember.part2.cs").Verify();

    [TestMethod]
    public void UnusedPrivateMember_Methods_EventHandler() =>
        // Event handler methods are not reported because in WPF an event handler
        // could be added through XAML and no warning will be generated if the
        // method is removed, which could lead to serious problems that are hard
        // to diagnose.
        builder.AddSnippet("""
            using System;
            public class NewClass
            {
               private void Handler(object sender, EventArgs e) { } // Compliant
            }
            public partial class PartialClass
            {
               private void Handler(object sender, EventArgs e) { } // intentional False Negative
            }
            """).VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_Unity3D_Ignored() =>
        builder.AddSnippet("""
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
            }
            """).VerifyNoIssues();

    [TestMethod]
    public void EntityFrameworkMigration_Ignored() =>
        builder.AddSnippet("""
            namespace EntityFrameworkMigrations
            {
                using Microsoft.EntityFrameworkCore.Migrations;

                public class SkipMigration : Migration
                {
                    private void SomeMethod(bool condition) { } // Compliant

                    protected override void Up(MigrationBuilder migrationBuilder) { }
                }
            }
            """)
        .AddReferences(EntityFrameworkCoreReferences("7.0.14"))
        .VerifyNoIssues();

    [TestMethod]
    [DataRow(ProjectType.Product)]
    [DataRow(ProjectType.Test)]
    public void UnusedPrivateMember(ProjectType projectType) =>
        builder.AddPaths("UnusedPrivateMember.cs").AddReferences(TestCompiler.ProjectTypeReference(projectType)).Verify();

#if NET

    [TestMethod]
    public void UnusedPrivateMember_CS_Latest() =>
        builder.AddPaths("UnusedPrivateMember.Latest.cs", "UnusedPrivateMember.Latest.Partial.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .AddReferences(MetadataReferenceFacade.NetStandard21)
            .AddReferences(MetadataReferenceFacade.MicrosoftExtensionsDependencyInjectionAbstractions)
            .Verify();

    [TestMethod]
    public void UnusedPrivateMember_TopLevelStatements() =>
        builder.AddPaths("UnusedPrivateMember.TopLevelStatements.cs").WithTopLevelStatements().Verify();

    [TestMethod]
    public void UnusedPrivateMemeber_EntityFramework_DontRaiseOnUnusedEntityPropertiesPrivateSetters() =>
        builder.AddSnippet("""
            // Repro https://github.com/SonarSource/sonar-dotnet/issues/9416
            using Microsoft.EntityFrameworkCore;

            internal class MyContext : DbContext
            {
                public DbSet<Blog> Blogs { get; set; }
            }

            public class Blog
            {
                public Blog(int id, string name)
                {
                    Name = name;
                }

                public int Id { get; private set; }         // Noncompliant FP
                public string Name { get; private set; }
            }
            """)
        .AddReferences(NuGetMetadataReference.MicrosoftEntityFrameworkCore("8.0.6"))
        .Verify();
#endif

    [TestMethod]
    public void UnusedPrivateMember_CodeFix() =>
        builder.AddPaths("UnusedPrivateMember.cs")
            .WithCodeFix<UnusedPrivateMemberCodeFix>()
            .WithCodeFixedPaths("UnusedPrivateMember.Fixed.cs", "UnusedPrivateMember.Fixed.Batch.cs")
            .VerifyCodeFix();

    [TestMethod]
    public void UnusedPrivateMember_UsedInGeneratedFile() =>
        builder.AddPaths("UnusedPrivateMember.CalledFromGenerated.cs", "UnusedPrivateMember.Generated.cs").VerifyNoIssues();

    [TestMethod]
    public void UnusedPrivateMember_Performance() =>
        // Once the NuGet packages are downloaded, the time to execute the analyzer on the given file is
        // about ~1 sec. It was reduced from ~11 min by skipping Guids when processing ObjectCreationExpression.
        // The threshold is set here to 30 seconds to avoid flaky builds due to slow build agents or network connections.
        builder.AddPaths("UnusedPrivateMember.Performance.cs")
            .AddReferences(EntityFrameworkCoreReferences("5.0.12"))   // The latest before 6.0.0 for .NET 6 that has Linq versioning collision issue
            .Invoking(x => x.VerifyNoIssues())
            .ExecutionTime().Should().BeLessOrEqualTo(30.Seconds());

    private static ImmutableArray<MetadataReference> EntityFrameworkCoreReferences(string entityFrameworkVersion) =>
        MetadataReferenceFacade.NetStandard
            .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreSqlServer(entityFrameworkVersion))
            .Concat(NuGetMetadataReference.MicrosoftEntityFrameworkCoreRelational(entityFrameworkVersion))
            .ToImmutableArray();
}
