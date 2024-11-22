/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Rules.CSharp;

namespace SonarAnalyzer.Test.Rules;

#if NET

[TestClass]
public class UseAspNetModelBindingTest
{
    private readonly VerifierBuilder builderAspNetCore = new VerifierBuilder<UseAspNetModelBinding>()
        .WithBasePath("AspNet")
        .WithOptions(ParseOptionsHelper.CSharpLatest)
        .AddReferences([
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
            AspNetCoreMetadataReference.MicrosoftAspNetCoreHttpFeatures,
            AspNetCoreMetadataReference.MicrosoftExtensionsPrimitives,
        ]);

    [TestMethod]
    public void UseAspNetModelBinding_NoRegistrationIfNotAspNet() =>
        new VerifierBuilder<UseAspNetModelBinding>().AddSnippet(string.Empty).VerifyNoIssues();

    [TestMethod]
    public void UseAspNetModelBinding_AspNetCore_CSharpLatest() =>
        builderAspNetCore.AddPaths("UseAspNetModelBinding_AspNetCore_Latest.cs").Verify();

    [DataTestMethod]
    [DataRow("form.Files", true)] // "form" is a parameter and as such IFormCollection binding was (probably) used which is an alternative to direct IFormFileCollection binding
    [DataRow("form?.Files", true)]
    [DataRow("form!.Files", true)]
    [DataRow("request.Form.Files", false)]
    [DataRow("request.Form?.Files", true)] // FN
    [DataRow("request.Form!.Files", false)]
    [DataRow("Request.Form.Files", false)]
    [DataRow("Request.Form!.Files", false)]
    [DataRow("Request.Form?.Files", true)] // FN
    public void UseAspNetModelBinding_AspNetCore_FormFileAccess(string filesAccess, bool compliant)
    {
        var builder = builderAspNetCore.AddSnippet($$""""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using System;
            using System.Linq;
            using System.Threading.Tasks;
            
            public class TestController : Controller
            {
                async Task NoncompliantKeyVariations(IFormCollection form, HttpRequest request)
                {
                    _ = {{filesAccess}}; // {{(compliant ? string.Empty : "//Noncompliant")}} 
                }
            }
            """");
        if (compliant)
        {
            builder.VerifyNoIssues();
        }
        else
        {
            builder.Verify();
        }
    }

    [DataTestMethod]
    [DataRow("Form")]
    [DataRow("Query")]
    [DataRow("RouteValues")]
    [DataRow("Headers")]
    public void UseAspNetModelBinding_NonCompliantAccess(string property) =>
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

                private static void HandleRequest(HttpRequest request)
                {
                    _ = request.{{property}}["id"]; // Noncompliant: Containing type is a controller
                    void LocalFunction()
                    {
                        _ = request.{{property}}["id"]; // Noncompliant: Containing type is a controller
                    }
                    static void StaticLocalFunction(HttpRequest request)
                    {
                        _ = request.{{property}}["id"]; // Noncompliant: Containing type is a controller
                    }
                }
            }
            """").Verify();

    [CombinatorialDataTestMethod]
    public void UseAspNetModelBinding_CompliantAccess(
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
            """).VerifyNoIssues();

    [DataTestMethod]
    [DataRow("Form")]
    [DataRow("Headers")]
    [DataRow("Query")]
    [DataRow("RouteValues")]
    public void UseAspNetModelBinding_DottedExpressions(string property) =>
        builderAspNetCore.AddSnippet($$"""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using Microsoft.AspNetCore.Routing;
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            public class TestController : Controller
            {
                HttpRequest ValidRequest => Request;
                IFormCollection Form => Request.Form;
                IHeaderDictionary Headers => Request.Headers;
                IQueryCollection Query => Request.Query;
                RouteValueDictionary RouteValues => Request.RouteValues;

                async Task DottedExpressions()
                {
                    _ = (true ? Request : Request).{{property}}["id"]; // Noncompliant
                    _ = ValidatedRequest().{{property}}["id"]; // Noncompliant
                    _ = ValidRequest.{{property}}["id"]; // Noncompliant
                    _ = {{property}}["id"];      // Noncompliant
                    _ = this.{{property}}["id"];                 // Noncompliant
                    _ = new TestController().{{property}}["id"]; // Noncompliant

                    _ = this.Request.{{property}}["id"]; // Noncompliant
                    _ = Request?.{{property}}?["id"]; // Noncompliant
                    _ = Request?.{{property}}?.TryGetValue("id", out _); // Noncompliant
                    _ = Request.{{property}}?.TryGetValue("id", out _); // Noncompliant
                    _ = Request.{{property}}?.TryGetValue("id", out _).ToString(); // Noncompliant
                    _ = HttpContext.Request.{{property}}["id"]; // Noncompliant
                    _ = Request.HttpContext.Request.{{property}}["id"]; // Noncompliant
                    _ = this.ControllerContext.HttpContext.Request.{{property}}["id"]; // Noncompliant
                    var r1 = HttpContext.Request;
                    _ = r1.{{property}}["id"]; // Noncompliant
                    var r2 = ControllerContext;
                    _ = r2.HttpContext.Request.{{property}}["id"]; // Noncompliant

                    HttpRequest ValidatedRequest() => Request;
                }
            }
            """).Verify();

    [DataTestMethod]
    [DataRow("public class MyController: Controller")]
    [DataRow("public class MyController: ControllerBase")]
    [DataRow("[Controller] public class My: Controller")]
    // [DataRow("public class MyController")] FN: Poco controller are not detected
    public void UseAspNetModelBinding_PocoController(string classDeclaration) =>
        builderAspNetCore.AddSnippet($$""""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            {{classDeclaration}}
            {
                public async Task Action([FromServices]IHttpContextAccessor httpContextAccessor)
                {
                    _ = httpContextAccessor.HttpContext.Request.Form["id"]; // Noncompliant
                }
            }
            """").Verify();

    [DataTestMethod]
    [DataRow("public class My")]
    [DataRow("[NonController] public class My: Controller")]
    [DataRow("[NonController] public class MyController: Controller")]
    public void UseAspNetModelBinding_NoController(string classDeclaration) =>
        builderAspNetCore.AddSnippet($$""""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            {{classDeclaration}}
            {
                public async Task Action([FromServices]IHttpContextAccessor httpContextAccessor)
                {
                    _ = httpContextAccessor.HttpContext.Request.Form["id"]; // Compliant
                }
            }
            """").VerifyNoIssues();

    [DataTestMethod]
    [DataRow("Form")]
    [DataRow("Headers")]
    [DataRow("Query")]
    [DataRow("RouteValues")]
    public void UseAspNetModelBinding_NoControllerHelpers(string property) =>
        builderAspNetCore.AddSnippet($$""""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            static class HttpRequestExtensions
            {
                public static void Ext(this HttpRequest request)
                {
                    _ = request.{{property}}["id"]; // Compliant: Not in a controller
                }
            }

            class RequestService
            {
                public HttpRequest Request { get; }

                public void HandleRequest(HttpRequest request)
                {
                    _ = Request.{{property}}["id"]; // Compliant: Not in a controller
                    _ = request.{{property}}["id"]; // Compliant: Not in a controller
                }
            }            
            """").VerifyNoIssues();

    [CombinatorialDataTestMethod]
    public void UseAspNetModelBinding_InheritanceAccess(
        [DataValues(
            ": Controller",
            ": ControllerBase",
            ": MyBaseController",
            ": MyBaseBaseController")]string baseList,
        [DataValues(
            """_ = Request.Form["id"]""",
            """_ = Request.Form.TryGetValue("id", out var _)""",
            """_ = Request.Headers["id"]""",
            """_ = Request.Query["id"]""",
            """_ = Request.RouteValues["id"]""")]string nonCompliantStatement) =>
        builderAspNetCore.AddSnippet($$""""
            using Microsoft.AspNetCore.Http;
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;
            using System;
            using System.Linq;
            using System.Threading.Tasks;

            public class MyBaseController : ControllerBase { }
            public class MyBaseBaseController : MyBaseController { }

            public class MyTestController {{baseList}}
            {
                public void Action()
                {
                    {{nonCompliantStatement}}; // Noncompliant
                }
            }
            """").Verify();
}
#endif
