/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using SonarAnalyzer.CSharp.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class ApiControllersShouldNotDeriveDirectlyFromControllerTest
{
#if NET
    private readonly VerifierBuilder builder = new VerifierBuilder<ApiControllersShouldNotDeriveDirectlyFromController>()
        .AddReferences(AspNetCoreReferences)
        .WithBasePath("AspNet");

    private static IEnumerable<MetadataReference> AspNetCoreReferences =>
    [
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
    ];

    [TestMethod]
    public void ApiControllersShouldNotDeriveDirectlyFromController_CS() =>
        builder.AddPaths("ApiControllersShouldNotDeriveDirectlyFromController.cs").Verify();

    [DataRow("""View()""")]
    [DataRow("""View("viewName")""")]
    [DataRow("""View(model)""")]
    [DataRow("""View("viewName", model)""")]
    [DataRow("""PartialView()""")]
    [DataRow("""PartialView("viewName")""")]
    [DataRow("""PartialView(model)""")]
    [DataRow("""PartialView("viewName", model)""")]
    [DataRow("""ViewComponent("foo")""")]
    [DataRow("""ViewComponent("foo", model)""")]
    [DataRow("""ViewComponent(typeof(object))""")]
    [DataRow("""ViewComponent(typeof(object), model)""")]
    [DataRow("""Json(model)""")]
    [DataRow("""Json(model, model)""")]
    [DataRow("""OnActionExecutionAsync(default(ActionExecutingContext), default(ActionExecutionDelegate))""")]
    [DataRow("""ViewData""")]
    [DataRow("""ViewBag""")]
    [DataRow("""TempData""")]
    [DataRow("""ViewData["foo"]""")]
    [DataRow("""ViewBag["foo"]""")]
    [DataRow("""TempData["foo"]""")]
    [TestMethod]
    public void ApiControllersShouldNotDeriveDirectlyFromController_DoesNotRaiseWithViewFunctionality(string invocation) =>
        builder.AddSnippet($$"""
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;

            [ApiController]
            public class Invocations : Controller    // Compliant
            {
                object model = null;
                public object Foo() => {{invocation}};
            }
            """).VerifyNoIssues();

    [DataRow("""OnActionExecuted(default(ActionExecutedContext))""")]
    [DataRow("""OnActionExecuting(default(ActionExecutingContext))""")]
    [TestMethod]
    public void ApiControllersShouldNotDeriveDirectlyFromController_DoesNotRaiseWithVoidInvocations(string assignment) =>
        builder.AddSnippet($$"""
            using Microsoft.AspNetCore.Mvc;
            using Microsoft.AspNetCore.Mvc.Filters;

            [ApiController]
            public class VoidInvocations : Controller           // Compliant
            {
                public void Foo() => {{assignment}};
            }
            """).VerifyNoIssues();

    [DataRow("public Test() => foo = View();", DisplayName = "Constructor")]
    [DataRow("~Test() => foo = View();", DisplayName = "Destructor")]
    [DataRow("object prop => View();", DisplayName = "PropertyGet")]
    [DataRow("object prop { set => _ = View(); }", DisplayName = "PropertySet")]
    [DataRow("object this[int index] => View();", DisplayName = "Indexer")]
    [TestMethod]
    public void ApiControllersShouldNotDeriveDirectlyFromController_DoesNotRaiseInDifferentConstructs(string code) =>
        builder.AddSnippet($$"""
            using Microsoft.AspNetCore.Mvc;

            [ApiController]
            public class Test : Controller  // Compliant
            {
                object foo;
                {{code}}
            }
            """).VerifyNoIssues();

    [TestMethod]
    public void ApiControllersShouldNotDeriveDirectlyFromController_CodeFix() =>
        builder
            .WithCodeFix<ApiControllersShouldNotDeriveDirectlyFromControllerCodeFix>()
            .AddPaths("ApiControllersShouldNotDeriveDirectlyFromControllerCodeFix.cs")
            .WithCodeFixedPaths("ApiControllersShouldNotDeriveDirectlyFromControllerCodeFix.Fixed.cs")
            .VerifyCodeFix();
#endif
}
