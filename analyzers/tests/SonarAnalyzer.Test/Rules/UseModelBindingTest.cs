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

    [TestMethod]
    public void UseModelBinding_AspNetCore_CS_Debug() =>
        builderAspNetCore
            .WithConcurrentAnalysis(false)
            .AddSnippet("""
                using Microsoft.AspNetCore.Http;
                using Microsoft.AspNetCore.Mvc;
                using Microsoft.AspNetCore.Mvc.Filters;
                using System;
                using System.Linq;
                using System.Threading.Tasks;

                public class OverridesController : Controller
                {
                    public void Action()
                    {
                        _ = Request.Form["id"]; // Noncompliant 
                    }
                    private void Undecidable(HttpContext context)
                    {
                        // Implementation: It might be difficult to distinguish between access to "Request" that originate from overrides vs. "Request" access that originate from action methods.
                        // This is especially true for "Request" which originate from parameters like here. We may need to redeclare such cases as FNs (see e.g HandleRequest above).
                        _ = context.Request.Form["id"]; // Undecidable: request may originate from an action method (which supports binding), or from one of the following overrides (which don't).
                    }
                    private void Undecidable(HttpRequest request)
                    {
                        _ = request.Form["id"]; // Undecidable: request may originate from an action method (which supports binding), or from one of the following overloads (which don't).
                    }
                    public override void OnActionExecuted(ActionExecutedContext context)
                    {
                        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
                    }
                    public override void OnActionExecuting(ActionExecutingContext context)
                    {
                        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
                    }
                    public override Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
                    {
                        _ = context.HttpContext.Request.Form["id"]; // Compliant: Model binding is not supported here
                        return base.OnActionExecutionAsync(context, next);
                    }
                }
                """).Verify();
#endif
}
