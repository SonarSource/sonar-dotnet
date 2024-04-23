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
public class CallModelStateIsValidTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<CallModelStateIsValid>()
        .WithBasePath("AspNet")
        .AddReferences([
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
            ..NuGetMetadataReference.SystemComponentModelAnnotations(Constants.NuGetLatestVersion)
        ]);

    [TestMethod]
    public void CallModelStateIsValid_CS() =>
        builder.AddPaths("CallModelStateIsValid.cs").Verify();

    [TestMethod]
    public void CallModelStateIsValid_AssemblyLevelControllerAttribute_CS() =>
        builder.AddSnippet("""
            using Microsoft.AspNetCore.Mvc;
            using System.ComponentModel.DataAnnotations;

            [assembly: ApiController]           // causes the ApiController attribute to be applied on every Controller class in the assembly

            public class Movie
            {
                [Required] public string Title { get; set; }
                [Range(1900, 2200)] public int Year { get; set; }
            }

            public class ControllerWithApiAttributeAtTheAssemblyLevel : ControllerBase
            {
                [HttpPost("/[controller]")]
                public string Add(Movie movie)  // Compliant - ApiController attribute is applied at the assembly level.
                {
                    return "Hello!";
                }
            }
            """).Verify();

    [DataTestMethod]
    [DataRow("!ModelState.IsValid")]
    [DataRow("ModelState?.IsValid is false")]
    [DataRow("!ModelState!.IsValid")]
    [DataRow("ModelState!?.IsValid is false")]
    [DataRow("ModelState?.IsValid! is false")]
    [DataRow("ModelState is { IsValid: false }")]
    [DataRow("this is { ModelState.IsValid: false }")]
    [DataRow("this is { ModelState: { IsValid: false } }")]
    [DataRow("this is { ModelState: { IsValid: not true } }")]
    [DataRow("ModelState.ValidationState != ModelValidationState.Valid")]
    [DataRow("ModelState.ValidationState == ModelValidationState.Invalid")]
    [DataRow("ModelState is { ValidationState: ModelValidationState.Invalid }")]
    public void CallModelStateIsValid_ValidatingState_CS(string condition) =>
        builder.AddSnippet($$"""
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.ModelBinding;
            using System.ComponentModel.DataAnnotations;

            public class Movie
            {
                [Required] public string Title { get; set; }
                [Range(1900, 2200)] public int Year { get; set; }
            }

            public class MovieController : ControllerBase
            {
                [HttpPost("/[controller]")]
                public IActionResult Add(Movie movie)  // Compliant
                {
                    if ({{condition}})
                    {
                        return BadRequest();
                    }
                    return Ok();
                }
            }
            """).WithOptions(ParseOptionsHelper.FromCSharp10).Verify();
}

#endif
