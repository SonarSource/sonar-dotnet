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

using System.IO;
using System.Xml.XPath;
using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules;

public abstract class ObjectShouldBeInitializedCorrectlyBase : SonarDiagnosticAnalyzer<SyntaxKind>
{
    protected abstract CSharpObjectInitializationTracker ObjectInitializationTracker { get; }

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    protected ObjectShouldBeInitializedCorrectlyBase(string diagnosticId) : base(diagnosticId) { }

    protected virtual bool IsDefaultConstructorSafe(SonarCompilationStartAnalysisContext context) => false;

    protected virtual void Initialize(TrackerInput input)
    {
        // This should be overriden by inheriting class that uses trackers
    }

    protected override void Initialize(SonarAnalysisContext context)
    {
        Initialize(new TrackerInput(context, AnalyzerConfiguration.AlwaysEnabled, Rule));
        context.RegisterCompilationStartAction(
            compilationStartContext =>
            {
                var isDefaultConstructorSafe = IsDefaultConstructorSafe(compilationStartContext);

                compilationStartContext.RegisterNodeAction(
                    c =>
                    {
                        var objectCreation = ObjectCreationFactory.Create(c.Node);
                        if (ObjectInitializationTracker.ShouldBeReported(objectCreation, c.Model, isDefaultConstructorSafe))
                        {
                            c.ReportIssue(SupportedDiagnostics[0], objectCreation.Expression);
                        }
                    },
                    SyntaxKind.ObjectCreationExpression,
                    SyntaxKindEx.ImplicitObjectCreationExpression);

                compilationStartContext.RegisterNodeAction(
                    c =>
                    {
                        var assignment = (AssignmentExpressionSyntax)c.Node;
                        if (ObjectInitializationTracker.ShouldBeReported(assignment, c.Model))
                        {
                            c.ReportIssue(SupportedDiagnostics[0], assignment);
                        }
                    },
                    SyntaxKind.SimpleAssignmentExpression);
            });
    }

    protected static bool IsWebConfigCookieSet(SonarCompilationStartAnalysisContext context, string attribute)
    {
        foreach (var fullPath in context.ProjectConfiguration().FilesToAnalyze.FindFiles("web.config"))
        {
            var webConfig = File.ReadAllText(fullPath);
            if (webConfig.Contains("<system.web>") && webConfig.ParseXDocument() is { } doc
                && doc.XPathSelectElements("configuration/system.web/httpCookies").Any(x => x.GetAttributeIfBoolValueIs(attribute, true) is not null))
            {
                return true;
            }
        }

        return false;
    }
}
