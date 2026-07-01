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

using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SonarAnalyzer.Core.Rules;

public abstract class DisablingRequestValidationBase : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S5753";
    private const string MessageFormat = "Ensure ASP.NET Request Validation is not disabled.";
    // See https://docs.microsoft.com/en-us/dotnet/api/system.web.configuration.httpruntimesection.requestvalidationmode
    private const int MinimumAcceptedRequestValidationModeValue = 4;

    private readonly DiagnosticDescriptor rule;

    protected abstract ILanguageFacade Language { get; }

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    protected DisablingRequestValidationBase() =>
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterSymbolAction(
            CheckController,
            SymbolKind.NamedType,
            SymbolKind.Method);

        context.RegisterCompilationAction(CheckWebConfig);
    }

    private void CheckController(SonarSymbolReportingContext context)
    {
        var attributes = context.Symbol.GetAttributes();
        if (attributes.IsEmpty)
        {
            return;
        }

        var attributeWithFalseParameter = attributes.FirstOrDefault(x =>
            x.ConstructorArguments.Length == 1
            && x.ConstructorArguments[0].Kind == TypedConstantKind.Primitive
            && x.ConstructorArguments[0].Value is bool enableValidationValue
            && !enableValidationValue
            && x.AttributeClass.Is(KnownType.System_Web_Mvc_ValidateInputAttribute));
        if (attributeWithFalseParameter is not null)
        {
            context.ReportIssue(Language.GeneratedCodeRecognizer, rule, attributeWithFalseParameter.ApplicationSyntaxReference.GetSyntax());
        }
    }

    private void CheckWebConfig(SonarCompilationReportingContext context)
    {
        foreach (var fullPath in context.WebConfigFiles())
        {
            var webConfig = File.ReadAllText(fullPath);
            if (webConfig.Contains("<system.web>") && webConfig.ParseXDocument() is { } doc)
            {
                ReportValidateRequest(context, doc, fullPath);
                ReportRequestValidationMode(context, doc, fullPath);
            }
        }
    }

    private void ReportValidateRequest(SonarCompilationReportingContext context, XDocument doc, string webConfigPath)
    {
        foreach (var pages in doc.XPathSelectElements("configuration/system.web/pages"))
        {
            if (pages.GetAttributeIfBoolValueIs("validateRequest", false) is { } validateRequest
                && validateRequest.CreateLocation(webConfigPath) is { } location)
            {
                context.ReportIssue(Language.GeneratedCodeRecognizer, rule, location);
            }
        }
    }

    private void ReportRequestValidationMode(SonarCompilationReportingContext context, XDocument doc, string webConfigPath)
    {
        foreach (var httpRuntime in doc.XPathSelectElements("configuration/system.web/httpRuntime"))
        {
            if (httpRuntime.Attribute("requestValidationMode") is { } requestValidationMode
                && decimal.TryParse(requestValidationMode.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out var value)
                && value < MinimumAcceptedRequestValidationModeValue
                && requestValidationMode.CreateLocation(webConfigPath) is { } location)
            {
                context.ReportIssue(Language.GeneratedCodeRecognizer, rule, location);
            }
        }
    }
}
