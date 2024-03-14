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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class UseModelBindingTest
{
#if NET
    private readonly VerifierBuilder builderAspNetCore = new VerifierBuilder<UseModelBinding>()
        .WithOptions(ParseOptionsHelper.FromCSharp12)
        .AddReferences([
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpFeatures,
            AspNetCoreMetadataReference.MicrosoftExtensionsPrimitives,
        ]);

    [TestMethod]
    public void UseModelBinding_AspNetCore_CS() =>
        builderAspNetCore.AddPaths("UseModelBinding_AspNetCore.cs").Verify();

    [DataTestMethod]
    [DataRow("Form")]
    [DataRow("Query")]
    [DataRow("RouteValues")]
    [DataRow("Headers")]
    public void NonCompliantAccess(string property) =>
        builderAspNetCore.AddSnippet($$""""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            public class TestController : Controller
            {
                async Task NoncompliantKeyVariations()
                {
                    _ = Request.{{property}}[@"key"];                                 // Noncompliant
                    _ = Request.{{property}}.TryGetValue(@"key", out _);              // Noncompliant
                    _ = Request.{{property}}["""key"""];                              // Noncompliant
                    _ = Request.{{property}}.TryGetValue("""key""", out _);           // Noncompliant
            
                    const string key = "id";
                    _ = Request.{{property}}[key];                                    // Noncompliant
                    _ = Request.{{property}}.TryGetValue(key, out _);                 // Noncompliant
                    _ = Request.{{property}}[$"prefix.{key}"];                        // Noncompliant
                    _ = Request.{{property}}.TryGetValue($"prefix.{key}", out _);     // Noncompliant
                    _ = Request.{{property}}[$"""prefix.{key}"""];                    // Noncompliant
                    _ = Request.{{property}}.TryGetValue($"""prefix.{key}""", out _); // Noncompliant
            
                    _ = Request.{{property}}[key: "id"];                              // Noncompliant
                    _ = Request.{{property}}.TryGetValue(value: out _, key: "id");    // Noncompliant
                }
            }
            """").Verify();

    [TestMethod]
    [CombinatorialData]
    public void CompliantAccess(
        [DataValues(
            "_ = {0}.Keys",
            "_ = {0}.Count",
            "foreach (var kvp in {0}) {{ }}",
            "_ = {0}.Select(x => x);",
            "_ = {0}[key];                    // Compliant: The accessed key is not a compile time constant")] string statementFormat,
        [DataValues("Request", "this.Request", "ControllerContext.HttpContext.Request", "request")] string request,
        [DataValues("Form", "Headers", "Query", "RouteValues")] string property,
        [DataValues("[FromForm]", "[FromQuery]", "[FromRoute]", "[FromHeader]")] string attribute) =>
        builderAspNetCore.AddSnippet($$"""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            public class TestController : Controller
            {
                async Task Compliant({{attribute}} string key, HttpRequest request)
                {
                    {{string.Format(statementFormat, $"{request}.{property}")}};
                }
            }
            """).Verify();
#endif
}
