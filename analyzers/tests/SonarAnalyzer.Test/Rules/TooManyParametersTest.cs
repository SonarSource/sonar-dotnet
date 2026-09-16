/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using CS = SonarAnalyzer.CSharp.Rules;
using VB = SonarAnalyzer.VisualBasic.Rules;

namespace SonarAnalyzer.Test.Rules;

[TestClass]
public class TooManyParametersTest
{
    private static readonly MetadataReference[] DependencyInjectionReferences =
        [
            ..NuGetMetadataReference.MicrosoftAspNetCoreMvcAbstractions(TestConstants.DotNetCore220Version),  // For IFilterMetadata, IActionFilter and the filter contexts
            ..NuGetMetadataReference.MicrosoftAspNetCoreMvcCore(TestConstants.DotNetCore220Version),        // For FromServicesAttribute
            ..NuGetMetadataReference.MicrosoftExtensionsDependencyInjectionAbstractions("8.0.1")            // For FromKeyedServicesAttribute
        ];

    // The DependencyInjectionTypes matrix declares the framework types itself, so these references cover the third-party ones with their real declarations.
    private static readonly MetadataReference[] HandlerReferences =
        [
            ..NuGetMetadataReference.Package("MassTransit.Abstractions", "8.3.7"),   // For IConsumer<TMessage>
            ..NuGetMetadataReference.Package("MediatR", "12.4.1"),                   // For IRequestHandler, INotificationHandler and IPipelineBehavior
            ..NuGetMetadataReference.Package("MediatR.Contracts", "2.0.1")           // For IRequest and INotification
        ];

    private static readonly MetadataReference[] AzureFunctionsReferences =
        [
            ..NuGetMetadataReference.Package("Microsoft.Azure.WebJobs.Extensions.Storage.Blobs", TestConstants.NuGetLatestVersion), // For BlobAttribute and BlobTriggerAttribute
            ..NuGetMetadataReference.Package("Microsoft.Azure.WebJobs.Extensions.CosmosDB", TestConstants.NuGetLatestVersion)       // For CosmosDBAttribute
        ];

    private readonly VerifierBuilder builderCSMax3 = new VerifierBuilder().AddAnalyzer(() => new CS.TooManyParameters { Maximum = 3 });
    private readonly VerifierBuilder builderVBMax3 = new VerifierBuilder().AddAnalyzer(() => new VB.TooManyParameters { Maximum = 3 });

    public static IEnumerable<object[]> DependencyInjectionTypes =>
        [
            ["MassTransit", "interface", "IConsumer", "IConsumer"],
            ["MediatR", "interface", "INotificationHandler<TNotification>", "INotificationHandler<object>"],
            ["MediatR", "interface", "IPipelineBehavior<TRequest, TResponse>", "IPipelineBehavior<object, object>"],
            ["MediatR", "interface", "IRequestHandler<TRequest>", "IRequestHandler<object>"],
            ["MediatR", "interface", "IRequestHandler<TRequest, TResponse>", "IRequestHandler<object, object>"],
            ["Microsoft.AspNet.SignalR", "class", "Hub", "Hub"],
            ["Microsoft.AspNetCore.Authorization", "class", "AuthorizationHandler<TRequirement>", "AuthorizationHandler<object>"],
            ["Microsoft.AspNetCore.Authorization", "interface", "IAuthorizationHandler", "IAuthorizationHandler"],
            ["Microsoft.AspNetCore.Http", "interface", "IMiddleware", "IMiddleware"],
            ["Microsoft.AspNetCore.Mvc", "class", "Controller", "Controller"],
            ["Microsoft.AspNetCore.Mvc", "class", "ControllerBase", "ControllerBase"],
            ["Microsoft.AspNetCore.Mvc.Filters", "interface", "IFilterMetadata", "IFilterMetadata"],
            ["Microsoft.AspNetCore.Mvc.RazorPages", "class", "PageModel", "PageModel"],
            ["Microsoft.AspNetCore.Mvc", "class", "ViewComponent", "ViewComponent"],
            ["Microsoft.AspNetCore.Razor.TagHelpers", "interface", "ITagHelper", "ITagHelper"],
            ["Microsoft.AspNetCore.Razor.TagHelpers", "class", "TagHelper", "TagHelper"],
            ["Microsoft.AspNetCore.SignalR", "class", "Hub", "Hub"],
            ["Microsoft.AspNetCore.SignalR", "class", "Hub<T>", "Hub<object>"],
            ["Microsoft.Extensions.Hosting", "class", "BackgroundService", "BackgroundService"],
            ["Microsoft.Extensions.Hosting", "interface", "IHostedService", "IHostedService"],
            ["System.Web.Http", "class", "ApiController", "ApiController"],
            ["System.Web.Mvc", "class", "Controller", "Controller"]
        ];

    [TestMethod]
    [DynamicData(nameof(DependencyInjectionTypes))]
    public void TooManyParameters_CS_DependencyInjectionTypes(string ns, string kind, string declaration, string type) =>
        builderCSMax3.AddSnippet($$"""
            namespace {{ns}}
            {
                public {{kind}} {{declaration}} { }
            }

            public class Direct : {{ns}}.{{type}}
            {
                public Direct() { }
                public Direct(int p1, int p2, int p3, int p4) { } // Compliant
                public void Method(int p1, int p2, int p3, int p4) { } // Noncompliant
            }

            public class Indirect : Direct
            {
                public Indirect(int p1, int p2, int p3, int p4) { } // Compliant
            }
            """)
            .Verify();

#if NET
    [TestMethod]
    [DynamicData(nameof(DependencyInjectionTypes))]
    public void TooManyParameters_CS_DependencyInjectionTypes_PrimaryConstructors(string ns, string kind, string declaration, string type) =>
        builderCSMax3.AddSnippet($$"""
            namespace {{ns}}
            {
                public {{kind}} {{declaration}} { }
            }

            public class Direct(int p1, int p2, int p3, int p4) : {{ns}}.{{type}} // Compliant
            {
                public void Method(int p1, int p2, int p3, int p4) { } // Noncompliant
            }

            public class Intermediate : {{ns}}.{{type}} { }
            public class Indirect(int p1, int p2, int p3, int p4) : Intermediate; // Compliant
            """)
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
#endif

    [TestMethod]
    [DynamicData(nameof(DependencyInjectionTypes))]
    public void TooManyParameters_VB_DependencyInjectionTypes(string ns, string kind, string declaration, string type) =>
        builderVBMax3.AddSnippet($"""
            Namespace {ns}
                Public {kind} {declaration.Replace("<", "(Of ").Replace(">", ")")}
                End {kind}
            End Namespace

            Public Class Direct
                {(kind == "class" ? "Inherits" : "Implements")} {ns}.{type.Replace("<", "(Of ").Replace(">", ")")}

                Public Sub New()
                End Sub

                Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
                End Sub

                Public Sub Method(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Noncompliant
                End Sub
            End Class

            Public Class Indirect
                Inherits Direct

                Public Sub New(p1 As Integer, p2 As Integer, p3 As Integer, p4 As Integer) ' Compliant
                End Sub
            End Class
            """)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_DependencyInjectionConstructors() =>
        builderCSMax3.AddPaths("TooManyParameters_DependencyInjectionConstructors.cs")
            .AddReferences(DependencyInjectionReferences)
            .Verify();

#if NET
    [TestMethod]
    public void TooManyParameters_CS_DependencyInjectionConstructors_Latest() =>
        builderCSMax3.AddPaths("TooManyParameters_DependencyInjectionConstructors.Latest.cs")
            .AddReferences(DependencyInjectionReferences)
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();
#endif

    [TestMethod]
    public void TooManyParameters_VB_DependencyInjectionConstructors() =>
        builderVBMax3.AddPaths("TooManyParameters_DependencyInjectionConstructors.vb")
            .AddReferences(DependencyInjectionReferences)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_DependencyInjectionHandlers() =>
        builderCSMax3.AddPaths("TooManyParameters_DependencyInjectionHandlers.cs")
            .AddReferences(HandlerReferences)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_CustomValues() =>
        builderCSMax3.AddPaths("TooManyParameters_CustomValues.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_CustomValues_TopLevelStatements() =>
         builderCSMax3.AddPaths("TooManyParameters_CustomValues.TopLevelStatements.cs")
            .WithTopLevelStatements()
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_CustomValues_Latest() =>
        builderCSMax3.AddPaths("TooManyParameters_CustomValues.Latest.cs")
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_FromServices() =>
        builderCSMax3.AddPaths("TooManyParameters_FromServices.cs")
            .AddReferences(DependencyInjectionReferences)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_FromServices_Latest() =>
        builderCSMax3.AddPaths("TooManyParameters_FromServices.Latest.cs")
            .AddReferences(DependencyInjectionReferences)
            .WithOptions(LanguageOptions.CSharpLatest)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_HotChocolate() =>
        builderCSMax3.AddPaths("TooManyParameters_HotChocolate.cs")
            .AddReferences(NuGetMetadataReference.HotChocolateAbstractions("13.9.14"))   // Pinned to 13.x, the last major version declaring ScopedServiceAttribute
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_Orleans() =>
        builderCSMax3.AddPaths("TooManyParameters_Orleans.cs")
            .AddReferences(NuGetMetadataReference.Package("Microsoft.Orleans.Runtime", TestConstants.NuGetLatestVersion))  // For PersistentStateAttribute
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_Dapr() =>
        builderCSMax3.AddPaths("TooManyParameters_Dapr.cs")
            .AddReferences(NuGetMetadataReference.Package("Dapr.AspNetCore", TestConstants.NuGetLatestVersion)) // For FromStateAttribute
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_AzureFunctions() =>
        builderCSMax3.AddPaths("TooManyParameters_AzureFunctions.cs")
            .AddReferences(AzureFunctionsReferences)
            .Verify();

    [TestMethod]
    public void TooManyParameters_VB_CustomValues() =>
        builderVBMax3.AddPaths("TooManyParameters_CustomValues.vb").Verify();

    [TestMethod]
    public void TooManyParameters_VB_FromServices() =>
        builderVBMax3.AddPaths("TooManyParameters_FromServices.vb")
            .AddReferences(DependencyInjectionReferences)
            .Verify();

    [TestMethod]
    public void TooManyParameters_CS_DefaultValues() =>
        new VerifierBuilder<CS.TooManyParameters>().AddPaths("TooManyParameters_DefaultValues.cs")
            .WithOptions(LanguageOptions.FromCSharp8)
            .Verify();

    [TestMethod]
    public void TooManyParameters_VB_DefaultValues() =>
        new VerifierBuilder<VB.TooManyParameters>().AddPaths("TooManyParameters_DefaultValues.vb").Verify();
}
