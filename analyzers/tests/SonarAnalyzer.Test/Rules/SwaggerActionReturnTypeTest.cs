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
#if NET

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class SwaggerActionReturnTypeTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<SwaggerActionReturnType>()
        .WithOptions(LanguageOptions.FromCSharp11)
        .AddReferences([
            ..NuGetMetadataReference.SwashbuckleAspNetCoreAnnotations(),
            ..NuGetMetadataReference.SwashbuckleAspNetCoreSwagger(),
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpResults,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
        ]);

    [TestMethod]
    public void SwaggerActionReturnType_CS() =>
        builder.AddPaths("SwaggerActionReturnType.cs").Verify();

    [TestMethod]
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
    public void SwaggerActionReturnType_IActionResult(string invocation) =>
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

    [TestMethod]
    [DataRow("""Accepted("uri")""")]
    [DataRow("""Accepted(uri)""")]
    public void SwaggerActionReturnType_IActionResult_Compliant(string invocation) =>
        builder
            .AddSnippet($$"""
                using System;
                using Microsoft.AspNetCore.Mvc;

                [ApiController]
                public class Foo : Controller
                {
                    [HttpGet("a")]
                    public IActionResult Method() =>
                        {{invocation}};

                    private Bar bar = new();
                    private Uri uri;
                }

                public class Bar {}
                """)
            .VerifyNoIssues();

    [TestMethod]
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
    public void SwaggerActionReturnType_IResult(string invocation) =>
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
            .VerifyNoIssues();
}

#endif
