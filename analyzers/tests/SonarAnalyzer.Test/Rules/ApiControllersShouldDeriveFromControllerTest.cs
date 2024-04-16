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
public class ApiControllersShouldDeriveFromControllerTest
{
    private readonly VerifierBuilder builder = new VerifierBuilder<ApiControllersShouldDeriveFromController>();

#if NET
    private static IEnumerable<MetadataReference> AspNetCoreReferences =>
    [
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcAbstractions,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcCore,
        AspNetCoreMetadataReference.MicrosoftAspNetCoreMvcViewFeatures,
    ];

    [TestMethod]
    public void ApiControllersShouldDeriveFromController_CS() =>
        builder.AddReferences(AspNetCoreReferences).AddPaths("ApiControllersShouldDeriveFromController.cs").Verify();

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
    [DataTestMethod]
    public void ApiControllersShouldDeriveFromController_DoesNotRaiseWithViewFunctionality(string assignment) =>
        builder.AddReferences(AspNetCoreReferences)
            .AddSnippet($$"""
                using Microsoft.AspNetCore.Mvc;

                [ApiController]
                public class Invocations : Controller    // Compliant
                {
                    object model = null;
                    public object Foo() => {{assignment}};
                }
                """)
            .VerifyNoIssueReported();

    [DataRow("""OnActionExecuted(default(ActionExecutedContext))""")]
    [DataRow("""OnActionExecuting(default(ActionExecutingContext))""")]
    [DataTestMethod]
    public void ApiControllersShouldDeriveFromController_DoesNotRaiseWithVoidInvocations(string assignment) =>
    builder.AddReferences(AspNetCoreReferences)
        .AddSnippet($$"""
                using Microsoft.AspNetCore.Mvc;
                using Microsoft.AspNetCore.Mvc.Filters;

                [ApiController]
                public class VoidInvocations : Controller           // Compliant
                {
                    public void Foo() => {{assignment}};
                }
                """)
        .VerifyNoIssueReported();

    [DataRow("""Constructor""", "public InConstructor() => foo = View()")]
    [DataRow("""Destructor""", "~InDestructor() => foo = View()")]
    [DataRow("""PropertyGet""", "object prop => View();")]
    [DataRow("""PropertySet""", "object prop { set => _ = View(); }")]
    [DataRow("""Indexer""", "object this[int index] => View()")]
    [DataTestMethod]
    public void ApiControllersShouldDeriveFromController_DoesNotRaiseInDifferentConstructs(string construct, string code) =>
        builder.AddReferences(AspNetCoreReferences)
            .AddSnippet($$"""
                using Microsoft.AspNetCore.Mvc;

                [ApiController]
                public class In{{construct}} : Controller  // Compliant
                {
                    object foo;
                    {{code}};
                }
                """)
        .VerifyNoIssueReported();
#endif

}
