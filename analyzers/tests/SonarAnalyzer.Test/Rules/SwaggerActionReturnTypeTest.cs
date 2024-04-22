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
public class SwaggerActionReturnTypeTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<SwaggerActionReturnType>()
        .WithOptions(ParseOptionsHelper.FromCSharp11)
        .AddReferences(NuGetMetadataReference.SwashbuckleAspNetCoreAnnotations())
        .AddReferences(NuGetMetadataReference.SwashbuckleAspNetCoreSwagger())
        .AddReferences([
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpResults,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
        ]);

    [TestMethod]
    public void SwaggerActionReturnType_CS() =>
        builder.AddPaths("SwaggerActionReturnType.cs").Verify();

    [DataTestMethod]
    [DataRow("""Ok(bar)""")]
    [DataRow("""Created("uri", bar)""")]
    [DataRow("""Created(new Uri("uri"), bar)""")]

    [DataRow("""CreatedAtAction("actionName", bar)""")]
    [DataRow("""CreatedAtAction("actionName", null, bar)""")]
    [DataRow("""CreatedAtAction("actionName", "controllerName", null, bar)""")]

    [DataRow("""CreatedAtRoute("routeName", bar)""")]
    [DataRow("""CreatedAtRoute("default(object)", bar)""")]
    [DataRow("""CreatedAtRoute("routeName", null, bar)""")]

    [DataRow("""Accepted(bar)""")]
    [DataRow("""Accepted("uri", bar)""")]
    [DataRow("""Accepted(new Uri("uri"), bar)""")]

    [DataRow("""AcceptedAtAction("actionName", bar)""")]
    [DataRow("""AcceptedAtAction("actionName", "controllerName", bar)""")]
    [DataRow("""AcceptedAtAction("actionName", "controllerName", null, bar)""")]

    [DataRow("""AcceptedAtRoute("routeName", null, foo)""")]
    [DataRow("""AcceptedAtRoute(default(object), foo)""")]
    public void RuleName_IActionResult(string invocation) =>
        builder
            .AddSnippet($$"""
                using System;
                using Microsoft.AspNetCore.Mvc;
                using Microsoft.AspNetCore.Builder;
                using Swashbuckle.AspNetCore.Annotations;
                using Microsoft.AspNetCore.Http.HttpResults;

                [ApiController]
                public class Foo : Controller
                {
                    [HttpGet("a")]
                    public IActionResult Method() =>    // Noncompliant
                        {{invocation}};                 // Secondary

                    private Bar bar = new();
                }

                public class Bar {}
                """)
            .Verify();
}

#endif
