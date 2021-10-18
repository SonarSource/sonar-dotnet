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

using System.IO;
using System.Linq;
using System.Xml.XPath;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.Helpers.Trackers;
using SonarAnalyzer.Wrappers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules
{
    public abstract class ObjectShouldBeInitializedCorrectlyBase : TrackerHotspotDiagnosticAnalyzer<SyntaxKind>
    {
        protected abstract CSharpObjectInitializationTracker ObjectInitializationTracker { get; }

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected ObjectShouldBeInitializedCorrectlyBase(IAnalyzerConfiguration configuration, string diagnosticId, string messageFormat)
            : base(configuration, diagnosticId, messageFormat) { }

        protected virtual bool IsDefaultConstructorSafe(SonarAnalysisContext context, AnalyzerOptions options) => false;

        protected override void Initialize(TrackerInput input)
        {
            // This should be overriden by inheriting class that uses trackers
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterCompilationStartAction(
                ccc =>
                {
                    if (!IsEnabled(ccc.Options))
                    {
                        return;
                    }

                    var isDefaultConstructorSafe = IsDefaultConstructorSafe(context, ccc.Options);

                    ccc.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var objectCreation = ObjectCreationFactory.Create(c.Node);
                            if (ObjectInitializationTracker.ShouldBeReported(objectCreation, c.SemanticModel, isDefaultConstructorSafe ))
                            {
                                c.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], objectCreation.Expression.GetLocation()));
                            }
                        },
                        SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression);

                    ccc.RegisterSyntaxNodeActionInNonGenerated(
                        c =>
                        {
                            var assignment = (AssignmentExpressionSyntax)c.Node;
                            if (ObjectInitializationTracker.ShouldBeReported(assignment, c.SemanticModel))
                            {
                                c.ReportIssue(Diagnostic.Create(SupportedDiagnostics[0], assignment.GetLocation()));
                            }
                        },
                        SyntaxKind.SimpleAssignmentExpression);
                });
        }

        protected static bool IsWebConfigCookieSet(SonarAnalysisContext context, AnalyzerOptions options, string attribute)
        {
            foreach (var fullPath in context.ProjectConfiguration(options).FilesToAnalyze.FindFiles("web.config"))
            {
                var webConfig = File.ReadAllText(fullPath);
                if (webConfig.Contains("<system.web>") && XmlHelper.ParseXDocument(webConfig) is { } doc
                    && doc.XPathSelectElements("configuration/system.web/httpCookies").Any(x => x.GetAttributeIfBoolValueIs(attribute, true) != null))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
