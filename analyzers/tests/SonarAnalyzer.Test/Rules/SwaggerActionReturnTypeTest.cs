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
    [DataRow("""AcceptedAtRoute("routeName", null, bar)""")]
    [DataRow("""AcceptedAtRoute(default(object), bar)""")]
    public void RuleName_IActionResult(string invocation) =>
        builder
            .AddSnippet($$"""
                using System;
                using Microsoft.AspNetCore.Mvc;

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

    [DataTestMethod]
    [DataRow("""Results.Ok(bar)""")]
    [DataRow("""Results.Ok((object) bar)""")]
    [DataRow("""Results.Ok(bar)""")]
    [DataRow("""Results.Created("uri", bar)""")]
    [DataRow("""Results.Created("uri", (object) bar)""")]
    [DataRow("""Results.Created(new Uri("uri"), bar)""")]
    [DataRow("""Results.Created(new Uri("uri"), (object) bar)""")]
    [DataRow("""Results.CreatedAtRoute(value: (object) bar)""")]
    [DataRow("""Results.CreatedAtRoute("", null, (object) bar)""")]
    [DataRow("""Results.CreatedAtRoute(value: bar)""")]
    [DataRow("""Results.CreatedAtRoute("", null, bar)""")]
    [DataRow("""Results.Accepted("uri", bar)""")]
    [DataRow("""Results.Accepted("uri", (object) bar)""")]
    [DataRow("""Results.AcceptedAtRoute(value: (object) bar)""")]
    [DataRow("""Results.AcceptedAtRoute("", null, (object) bar)""")]
    [DataRow("""Results.AcceptedAtRoute(value: bar)""")]
    [DataRow("""Results.AcceptedAtRoute("", null, bar)""")]
    public void RuleName_IResult(string invocation) =>
        builder
            .AddSnippet($$"""
                using System;
                using Microsoft.AspNetCore.Mvc;
                using Microsoft.AspNetCore.Http;

                [ApiController]
                public class Foo : Controller
                {
                    [HttpGet("a")]
                    public IResult Method() =>    // Noncompliant
                        {{invocation}};           // Secondary

                    private Bar bar = new();
                }

                public class Bar {}
                """)
            .Verify();

    [TestMethod]
    public void ApiConventionType_AssemblyLevel() =>
        builder
            .AddSnippet("""
                using Microsoft.AspNetCore.Mvc;

                [assembly: ApiConventionType(typeof(DefaultApiConventions))]
                namespace MyNameSpace;

                [ApiController]
                public class Foo : Controller
                {
                    [HttpGet("a")]
                    public IActionResult Method() => Ok(bar); // Compliant
                    private Bar bar = new();
                }

                public class Bar {}
                """)
            .Verify();
}

#endif
