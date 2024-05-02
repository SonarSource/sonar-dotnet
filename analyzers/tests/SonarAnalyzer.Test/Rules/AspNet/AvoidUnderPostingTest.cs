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

#if NET

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class AvoidUnderPostingTest
{
    private static readonly IEnumerable<MetadataReference> AspNetReferences = [
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
                AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
                ..NuGetMetadataReference.SystemComponentModelAnnotations(Constants.NuGetLatestVersion)];

    private readonly VerifierBuilder builder = new VerifierBuilder<AvoidUnderPosting>()
            .WithBasePath("AspNet")
            .AddReferences(AspNetReferences);

    [TestMethod]
    public void AvoidUnderPosting_CSharp() =>
        builder.AddPaths("AvoidUnderPosting.cs").Verify();

    [TestMethod]
    public void AvoidUnderPosting_CSharp8() =>
        builder.AddPaths("AvoidUnderPosting.CSharp8.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp8)
            .Verify();

    [TestMethod]
    public void AvoidUnderPosting_CSharp9() =>
        builder.AddPaths("AvoidUnderPosting.CSharp9.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp9)
            .Verify();

    [TestMethod]
    public void AvoidUnderPosting_CSharp12() =>
        builder.AddPaths("AvoidUnderPosting.CSharp12.cs")
            .WithOptions(ParseOptionsHelper.FromCSharp12)
            .Verify();

    [DataTestMethod]
    [DataRow("class")]
    [DataRow("struct")]
    [DataRow("record")]
    [DataRow("record struct")]
    public void AvoidUnderPosting_EnclosingTypes_CSharp(string enclosingType) =>
        builder.AddSnippet($$"""
            using Microsoft.AspNetCore.Mvc;
            using System.ComponentModel.DataAnnotations;

            public {{enclosingType}} Model
            {
                public int ValueProperty { get; set; }                      // Noncompliant
                public int? NullableValueProperty { get; set; }             // Compliant
                [Required] public int RequiredValueProperty { get; set; }   // Compliant
            }

            public class ControllerClass : Controller
            {
                [HttpPost] public IActionResult Create(Model model) => View(model);
            }
            """)
        .WithOptions(ParseOptionsHelper.FromCSharp10)
        .Verify();

    [DataTestMethod]
    [DataRow("HttpDelete")]
    [DataRow("HttpGet")]
    [DataRow("HttpPost")]
    [DataRow("HttpPut")]
    public void AvoidUnderPosting_HttpHandlers_CSharp(string attribute) =>
        builder.AddSnippet($$"""
            using Microsoft.AspNetCore.Mvc;
            using System.ComponentModel.DataAnnotations;

            public class Model
            {
                public int ValueProperty { get; set; }  // Noncompliant
            }

            public class ControllerClass : Controller
            {
                [{{attribute}}] public IActionResult Create(Model model) => View(model);
            }
            """).Verify();

    [TestMethod]
    public void AvoidUnderPosting_ModelInDifferentProject_CSharp()
    {
        const string modelCode = """
            namespace Models
            {
                public class Person
                {
                    public int Age { get; set; }    // FN - Roslyn can't raise an issue when the location is in different project than the one being analyzed
                }
            }
            """;
        const string controllerCode = """
            using Microsoft.AspNetCore.Mvc;
            using Models;

            namespace Controllers
            {
                public class PersonController : Controller
                {
                    [HttpPost] public IActionResult Post(Person person) => View(person);
                }
            }
            """;
        var solution = SolutionBuilder.Create()
            .AddProject(AnalyzerLanguage.CSharp)
            .AddSnippet(modelCode)
            .Solution
            .AddProject(AnalyzerLanguage.CSharp)
            .AddProjectReference(x => x.ProjectIds[0])
            .AddReferences(AspNetReferences)
            .AddSnippet(controllerCode)
            .Solution;
        var compiledAspNetProject = solution.Compile()[1];
        DiagnosticVerifier.Verify(compiledAspNetProject, new AvoidUnderPosting());
    }
}

#endif
