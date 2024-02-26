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

using static Roslyn.Utilities.SonarAnalyzer.Shared.LoggingFrameworkMethods;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class ExceptionsShouldBePassedCorrectly : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6668";
    private const string MessageFormat = "Logging arguments should be passed to the correct parameter.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var invocation = (InvocationExpressionSyntax)c.Node;
                if (LoggingInvocationSymbol(invocation, c.SemanticModel) is { } invocationSymbol)
                {
                    var exceptionParameterIndex = ExceptionParameterIndex(invocationSymbol);
                    var exceptionArguments = invocation.ArgumentList.Arguments
                        .Where(x => c.SemanticModel.GetTypeInfo(x.Expression).Type.DerivesFrom(KnownType.System_Exception))
                        .ToArray();

                    // Do not raise if there is at least one argument in the right place.
                    if (Array.Exists(exceptionArguments, x => x.GetArgumentIndex() == exceptionParameterIndex))
                    {
                        return;
                    }

                    foreach (var wrongArgument in exceptionArguments.Where(x => x.GetArgumentIndex() != exceptionParameterIndex))
                    {
                        c.ReportIssue(Diagnostic.Create(Rule, wrongArgument.GetLocation()));
                    }
                }
            },
            SyntaxKind.InvocationExpression);

    private static IMethodSymbol LoggingInvocationSymbol(InvocationExpressionSyntax invocation, SemanticModel model) =>
        MicrosoftExtensionsLogging.Contains(invocation.GetIdentifier().ToString())
        && model.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
        && methodSymbol.HasContainingType(KnownType.Microsoft_Extensions_Logging_LoggerExtensions, false)
            ? methodSymbol
            : null;

    private static int ExceptionParameterIndex(IMethodSymbol invocationSymbol)
    {
        var exceptionParameter = invocationSymbol.Parameters.FirstOrDefault(x => x.Type.DerivesFrom(KnownType.System_Exception));
        return invocationSymbol.Parameters.IndexOf(exceptionParameter);
    }
}
