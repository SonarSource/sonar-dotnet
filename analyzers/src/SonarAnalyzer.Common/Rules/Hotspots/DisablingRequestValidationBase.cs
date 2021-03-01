/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class DisablingRequestValidationBase : HotspotDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S5753";
        private const string MessageFormat = "Make sure disabling ASP.NET Request Validation feature is safe here.";
        // See https://docs.microsoft.com/en-us/dotnet/api/system.web.configuration.httpruntimesection.requestvalidationmode
        private const int MinimumAcceptedRequestValidationModeValue = 4;

        private readonly DiagnosticDescriptor rule;
        private readonly string rootPath;

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected DisablingRequestValidationBase(System.Resources.ResourceManager rspecResources, IAnalyzerConfiguration configuration, string rootPath) : base(configuration)
        {
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();
            this.rootPath = rootPath;
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(c =>
                {
                    if (!IsEnabled(c.Options))
                    {
                        return;
                    }
                    var attributes = c.Symbol.GetAttributes();
                    if (attributes.IsEmpty)
                    {
                        return;
                    }

                    var attributeWithFalseParameter = attributes.FirstOrDefault(a =>
                        a.ConstructorArguments.Length == 1
                        && a.ConstructorArguments[0].Kind == TypedConstantKind.Primitive
                        && a.ConstructorArguments[0].Value is bool b
                        && b == false
                        && a.AttributeClass.Is(KnownType.System_Web_Mvc_ValidateInputAttribute));
                    if (attributeWithFalseParameter != null)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attributeWithFalseParameter.ApplicationSyntaxReference.GetSyntax().GetLocation()));
                    }
                },
                SymbolKind.NamedType,
                SymbolKind.Method);

            context.RegisterCompilationAction(c =>
            {
                if (!IsEnabled(c.Options))
                {
                    return;
                }

                foreach (var fullPath in GetWebConfigFilePathsRecursively(rootPath))
                {
                    var webConfig = File.ReadAllText(fullPath);
                    if (webConfig.Contains("<system.web>") && ParseXDocument(webConfig) is { } doc)
                    {
                        ReportValidateRequest(doc, fullPath, c);
                        ReportRequestValidationMode(doc, fullPath, c);
                    }
                }
            });
        }

        private void ReportValidateRequest(XDocument doc, string webConfigPath, CompilationAnalysisContext c)
        {
            foreach (var pages in doc.XPathSelectElements("configuration/system.web/pages"))
            {
                if (pages.Attribute("validateRequest") is { } validateRequest
                    && validateRequest.Value.Equals("false", StringComparison.OrdinalIgnoreCase)
                    && CreateLocation(webConfigPath, pages, validateRequest.ToString()) is { } location)
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
                }
            }
        }

        private void ReportRequestValidationMode(XDocument doc, string webConfigPath, CompilationAnalysisContext c)
        {
            foreach (var httpRuntime in doc.XPathSelectElements("configuration/system.web/httpRuntime"))
            {
                if (httpRuntime.Attribute("requestValidationMode") is { } requestValidationMode
                    && decimal.TryParse(requestValidationMode.Value, out var value)
                    && value < MinimumAcceptedRequestValidationModeValue
                    && CreateLocation(webConfigPath, httpRuntime, requestValidationMode.ToString()) is { } location)
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, location));
                }
            }
        }

        private static IEnumerable<string> GetWebConfigFilePathsRecursively(string rootPath) =>
            Directory.GetFiles(rootPath, "web.config", SearchOption.AllDirectories).Select(p => Path.GetFullPath(p));

        private static Location CreateLocation(string path, XNode element, string attribute)
        {
            var lineInfo = (IXmlLineInfo)element;
            if (lineInfo.HasLineInfo())
            {
                // the IXmlLineInfo.LineNumber is 1-based, whereas Roslyn is zero-based
                var lineNumber = lineInfo.LineNumber - 1;
                // IXmlLineInfo.LinePosition is 1-based
                var start = lineInfo.LinePosition + element.ToString().IndexOf(attribute) - 2;
                var end = start + attribute.Length;
                var linePos = new LinePositionSpan(new LinePosition(lineNumber, start), new LinePosition(lineNumber, end));
                return Location.Create(path, new TextSpan(lineNumber, attribute.Length), linePos);
            }
            return null;
        }

        private static XDocument ParseXDocument(string text)
        {
            try
            {
                return XDocument.Parse(text, LoadOptions.SetLineInfo);
            }
            catch
            {
                return null;
            }
        }
    }
}
