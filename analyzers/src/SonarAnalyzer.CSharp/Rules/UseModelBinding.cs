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
    private const string UseModelBindingMessage = "Use model binding instead of accessing the raw request data";
    private const string UseIFormFileBindingMessage = "Use IFormFile or IFormFileCollection binding instead";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            var argumentTracker = new CSharpArgumentTracker();
            var propertyTracker = new CSharpPropertyAccessTracker();
            var argumentDescriptors = new List<ArgumentDescriptor>();
            if (compilationStartContext.Compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Mvc_ControllerAttribute) is { } controllerAttribute)
            {
                // ASP.Net core
                argumentDescriptors.Add(ArgumentDescriptor.ElementAccess(// Request.Form["id"]
                    invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                    invokedIndexerExpression: "Form",
                    parameterConstraint: parameter => parameter.IsType(KnownType.System_String) && IsGetterParameter(parameter),
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.MethodInvocation(// Request.Form.TryGetValue("id", out _)
                    invokedType: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                    methodName: "TryGetValue",
                    parameterName: "key",
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.MethodInvocation(// Request.Form.ContainsKey("id")
                    invokedType: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                    methodName: "ContainsKey",
                    parameterName: "key",
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.ElementAccess(// Request.Headers["id"]
                    invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IHeaderDictionary,
                    invokedIndexerExpression: "Headers",
                    parameterConstraint: parameter => parameter.IsType(KnownType.System_String) && IsGetterParameter(parameter),
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.MethodInvocation(// Request.Headers.TryGetValue("id", out _)
                    invokedMethodSymbol: x => IsIDictionaryStringStringValuesInvocation(x, "TryGetValue"),
                    invokedMemberNameConstraint: (name, comparison) => string.Equals(name, "TryGetValue", comparison),
                    parameterConstraint: x => string.Equals(x.Name, "key", StringComparison.Ordinal),
                    argumentPosition: x => x == 0,
                    refKind: null));
                argumentDescriptors.Add(ArgumentDescriptor.MethodInvocation(// Request.Headers.ContainsKey("id")
                    invokedMethodSymbol: x => IsIDictionaryStringStringValuesInvocation(x, "ContainsKey"),
                    invokedMemberNameConstraint: (name, comparison) => string.Equals(name, "ContainsKey", comparison),
                    parameterConstraint: x => string.Equals(x.Name, "key", StringComparison.Ordinal),
                    argumentPosition: x => x == 0,
                    refKind: null));
                argumentDescriptors.Add(ArgumentDescriptor.ElementAccess(// Request.Query["id"]
                    invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IQueryCollection,
                    invokedIndexerExpression: "Query",
                    parameterConstraint: parameter => parameter.IsType(KnownType.System_String) && IsGetterParameter(parameter),
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.MethodInvocation(// Request.Query.TryGetValue("id", out _)
                    invokedType: KnownType.Microsoft_AspNetCore_Http_IQueryCollection,
                    methodName: "TryGetValue",
                    parameterName: "key",
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.ElementAccess(// Request.RouteValues["id"]
                    invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Routing_RouteValueDictionary,
                    invokedIndexerExpression: "RouteValues",
                    parameterConstraint: parameter => parameter.IsType(KnownType.System_String) && IsGetterParameter(parameter),
                    argumentPosition: 0));
                argumentDescriptors.Add(ArgumentDescriptor.MethodInvocation(// Request.RouteValues.TryGetValue("id", out _)
                    invokedType: KnownType.Microsoft_AspNetCore_Routing_RouteValueDictionary,
                    methodName: "TryGetValue",
                    parameterName: "key",
                    argumentPosition: 0));
            }

            var propertyAccessDescriptors = new List<MemberDescriptor>
            {
                new(KnownType.Microsoft_AspNetCore_Http_IFormCollection, "Files"), // Request.Form.Files...
            };
            // TODO: Add descriptors for Asp.Net MVC 4.x
            if (argumentDescriptors.Any() || propertyAccessDescriptors.Any())
            {
                compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
                {
                    if (symbolStartContext.Symbol is INamedTypeSymbol namedType
                        && namedType.IsControllerType())
                    {
                        if (argumentDescriptors.Any())
                        {
                            symbolStartContext.RegisterSyntaxNodeAction(nodeContext =>
                            {
                                var argument = (ArgumentSyntax)nodeContext.Node;
                                var context = new ArgumentContext(argument, nodeContext.SemanticModel);
                                if (argumentDescriptors.Any(x => argumentTracker.MatchArgument(x)(context))
                                    && nodeContext.SemanticModel.GetConstantValue(argument.Expression) is { HasValue: true, Value: string })
                                {
                                    nodeContext.ReportIssue(Diagnostic.Create(Rule, GetPrimaryLocation(argument), UseModelBindingMessage));
                                }
                            }, SyntaxKind.Argument);
                        }
                        if (propertyAccessDescriptors.Any())
                        {
                            symbolStartContext.RegisterSyntaxNodeAction(nodeContext =>
                            {
                                var memberAccess = (MemberAccessExpressionSyntax)nodeContext.Node;
                                var context = new PropertyAccessContext(memberAccess, nodeContext.SemanticModel, memberAccess.Name.Identifier.ValueText);
                                if (propertyTracker.MatchProperty([.. propertyAccessDescriptors])(context))
                                {
                                    nodeContext.ReportIssue(Diagnostic.Create(Rule, memberAccess.GetLocation(), UseIFormFileBindingMessage));
                                }
                            }, SyntaxKind.SimpleMemberAccessExpression);
                        }
                    }
                }, SymbolKind.NamedType);
            }
        });
    }

    private static Location GetPrimaryLocation(ArgumentSyntax argument) =>
        argument switch
        {
            { Parent: BracketedArgumentListSyntax { Parent: ElementAccessExpressionSyntax { Expression: { } expression } } } => expression.GetLocation(),
            { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: { } expression } } } => expression.GetLocation(),
            _ => argument.GetLocation(),
        };

    private static bool IsGetterParameter(IParameterSymbol parameter) =>
        parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet };

    private static bool IsIDictionaryStringStringValuesInvocation(IMethodSymbol method, string name) =>
        method.Is(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, name)
            && method.ContainingType.TypeArguments is { Length: 2 } typeArguments
            && typeArguments[0].Is(KnownType.System_String)
            && typeArguments[1].Is(KnownType.Microsoft_Extensions_Primitives_StringValues);
}
