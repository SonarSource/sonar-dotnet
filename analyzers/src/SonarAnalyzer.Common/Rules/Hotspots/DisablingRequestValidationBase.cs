/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.Globalization;
using System.IO;
using System.Xml.Linq;
using System.Xml.XPath;

namespace SonarAnalyzer.Rules
{
    public abstract class DisablingRequestValidationBase : HotspotDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5753";
        private const string MessageFormat = "Make sure disabling ASP.NET Request Validation feature is safe here.";
        // See https://docs.microsoft.com/en-us/dotnet/api/system.web.configuration.httpruntimesection.requestvalidationmode
        private const int MinimumAcceptedRequestValidationModeValue = 4;

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade Language { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected DisablingRequestValidationBase(IAnalyzerConfiguration configuration) : base(configuration) =>
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(CheckController,
                SymbolKind.NamedType,
                SymbolKind.Method);

            context.RegisterCompilationAction(CheckWebConfig);
        }

        private void CheckController(SonarSymbolReportingContext context)
        {
            if (!IsEnabled(context.Options))
            {
                return;
            }
            var attributes = context.Symbol.GetAttributes();
            if (attributes.IsEmpty)
            {
                return;
            }

            var attributeWithFalseParameter = attributes.FirstOrDefault(a =>
                a.ConstructorArguments.Length == 1
                && a.ConstructorArguments[0].Kind == TypedConstantKind.Primitive
                && a.ConstructorArguments[0].Value is bool enableValidationValue
                && !enableValidationValue
                && a.AttributeClass.Is(KnownType.System_Web_Mvc_ValidateInputAttribute));
            if (attributeWithFalseParameter != null)
            {
                context.ReportIssue(Language.GeneratedCodeRecognizer, CreateDiagnostic(rule, attributeWithFalseParameter.ApplicationSyntaxReference.GetSyntax().GetLocation()));
            }
        }

        private void CheckWebConfig(SonarCompilationReportingContext context)
        {
            if (!IsEnabled(context.Options))
            {
                return;
            }

            foreach (var fullPath in context.WebConfigFiles())
            {
                var webConfig = File.ReadAllText(fullPath);
                if (webConfig.Contains("<system.web>") && XmlHelper.ParseXDocument(webConfig) is { } doc)
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
                    context.ReportIssue(Language.GeneratedCodeRecognizer, CreateDiagnostic(rule, location));
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
                    context.ReportIssue(Language.GeneratedCodeRecognizer, CreateDiagnostic(rule, location));
                }
            }
        }
    }
}
