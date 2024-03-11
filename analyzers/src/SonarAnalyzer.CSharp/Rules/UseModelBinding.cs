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

using SonarAnalyzer.Helpers.Trackers;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseModelBinding : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S6932";
    private const string MessageFormat = "Use model binding instead of accessing the raw request data";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            var tracker = new CSharpArgumentTracker();
            var argumentDescriptors = new List<ArgumentDescriptor>();
            if (compilationStartContext.Compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Mvc_ControllerAttribute) is { } controllerAttribute)
            {
                // ASP.Net core
                argumentDescriptors.Add(ArgumentDescriptor.ElementAccess(
                    invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                    invokedIndexerExpression: "Form",
                    parameterConstraint: parameter => parameter.IsType(KnownType.System_String) && parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet },
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.MethodInvocation(
                    invokedType: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                    methodName: "TryGetValue",
                    parameterName: "key",
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.MethodInvocation(
                    invokedType: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                    methodName: "ContainsKey",
                    parameterName: "key",
                    argumentPosition: 0));
            }
            if (argumentDescriptors.Any())
            {
                compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
                {
                    if (symbolStartContext.Symbol is INamedTypeSymbol namedType
                        && namedType.IsControllerType())
                    {
                        symbolStartContext.RegisterSyntaxNodeAction(nodeContext =>
                        {
                            var argument = (ArgumentSyntax)nodeContext.Node;
                            var context = new ArgumentContext(argument, nodeContext.SemanticModel);
                            if (argumentDescriptors.Any(x => tracker.MatchArgument(x)(context))
                                && nodeContext.SemanticModel.GetConstantValue(argument.Expression) is { HasValue: true, Value: string })
                            {
                                nodeContext.ReportIssue(Diagnostic.Create(Rule, GetPrimaryLocation(argument)));
                            }
                        }, SyntaxKind.Argument);
                    }
                }, SymbolKind.NamedType);
            }
        });
    }

    private Location GetPrimaryLocation(ArgumentSyntax argument) =>
        argument switch
        {
            { Parent: BracketedArgumentListSyntax { Parent: ElementAccessExpressionSyntax { Expression: { } expression } } } => expression.GetLocation(),
            { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: { } expression } } } => expression.GetLocation(),
            _ => argument.GetLocation(),
        };
}
