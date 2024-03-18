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

using System.Collections.Concurrent;

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseAspNetModelBinding : SonarDiagnosticAnalyzer<SyntaxKind>
{
    private const string DiagnosticId = "S6932";
    private const string UseAspNetModelBindingMessage = "Use model binding instead of accessing the raw request data";
    private const string UseIFormFileBindingMessage = "Use IFormFile or IFormFileCollection binding instead";

    protected override string MessageFormat => "{0}";

    protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

    public UseAspNetModelBinding() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context)
    {
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            var (argumentDescriptors, propertyAccessDescriptors) = GetDescriptors(compilationStartContext.Compilation);
            if (argumentDescriptors.Any() || propertyAccessDescriptors.Any())
            {
                compilationStartContext.RegisterSymbolStartAction(symbolStartContext =>
                {
                    // If the user overrides any action filters, model binding may not be working as expected. Then we do not want to raise on expressions that originate from parameters.
                    // See the OverridesController.Undecidable test cases for details.
                    var hasOverrides = false;
                    var controllerCandidates = new ConcurrentStack<ReportCandidate>(); // In SymbolEnd, we filter the candidates based on the overriding we learn on the go.
                    if (symbolStartContext.Symbol is INamedTypeSymbol namedType && namedType.IsControllerType())
                    {
                        symbolStartContext.RegisterCodeBlockStartAction<SyntaxKind>(codeBlockStart =>
                            hasOverrides |= RegisterCodeBlockActions(codeBlockStart, argumentDescriptors, propertyAccessDescriptors, controllerCandidates));
                    }
                    symbolStartContext.RegisterSymbolEndAction(symbolEnd =>
                    {
                        foreach (var candidate in controllerCandidates.Where(x => !(hasOverrides && x.OriginatesFromParameter)))
                        {
                            symbolEnd.ReportIssue(Diagnostic.Create(Rule, candidate.Location, candidate.Message));
                        }
                    });
                }, SymbolKind.NamedType);
            }
        });
    }

    private bool RegisterCodeBlockActions(SonarCodeBlockStartAnalysisContext<SyntaxKind> codeBlockStart,
        ArgumentDescriptor[] argumentDescriptors, MemberDescriptor[] propertyAccessDescriptors,
        ConcurrentStack<ReportCandidate> controllerCandidates)
    {
        if (codeBlockStart.OwningSymbol is IMethodSymbol method && IsOverridingFilterMethods(method))
        {
            // We do not want to raise in ActionFilter overrides. The SymbolEndAction needs to be made aware, that there are
            // ActionFilter overrides, so it can filter out some candidates.
            return true;
        }
        // Within a single code block, access via constant and variable keys could be mixed
        // We only want to raise, if all access were done via constants
        var allConstantAccess = true;
        var codeBlockCandidates = new ConcurrentStack<ReportCandidate>();
        if (argumentDescriptors.Any())
        {
            codeBlockStart.RegisterNodeAction(nodeContext =>
            {
                var argument = (ArgumentSyntax)nodeContext.Node;
                var context = new ArgumentContext(argument, nodeContext.SemanticModel);
                if (allConstantAccess && Array.Exists(argumentDescriptors, x => Language.Tracker.Argument.MatchArgument(x)(context)))
                {
                    allConstantAccess &= nodeContext.SemanticModel.GetConstantValue(argument.Expression) is { HasValue: true, Value: string };
                    codeBlockCandidates.Push(new(UseAspNetModelBindingMessage, GetPrimaryLocation(argument), OriginatesFromParameter(nodeContext.SemanticModel, argument)));
                }
            }, SyntaxKind.Argument);
        }
        if (propertyAccessDescriptors.Any())
        {
            codeBlockStart.RegisterNodeAction(nodeContext =>
            {
                var memberAccess = (MemberAccessExpressionSyntax)nodeContext.Node;
                var context = new PropertyAccessContext(memberAccess, nodeContext.SemanticModel, memberAccess.Name.Identifier.ValueText);
                if (Language.Tracker.PropertyAccess.MatchProperty(propertyAccessDescriptors)(context))
                {
                    codeBlockCandidates.Push(new(UseIFormFileBindingMessage, memberAccess.GetLocation(), OriginatesFromParameter(nodeContext.SemanticModel, memberAccess)));
                }
            }, SyntaxKind.SimpleMemberAccessExpression);
        }
        codeBlockStart.RegisterCodeBlockEndAction(codeBlockEnd =>
        {
            if (allConstantAccess)
            {
                controllerCandidates.PushRange([.. codeBlockCandidates]);
            }
        });
        return false;
    }

    private static (ArgumentDescriptor[] ArgumentDescriptors, MemberDescriptor[] PropertyAccessDescriptors) GetDescriptors(Compilation compilation)
    {
        var argumentDescriptors = new List<ArgumentDescriptor>();
        var propertyAccessDescriptors = new List<MemberDescriptor>();
        if (compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Mvc_ControllerAttribute) is { })
        {
            AddAspNetCoreDescriptors(argumentDescriptors, propertyAccessDescriptors);
        }
        // TODO: Add descriptors for Asp.Net MVC 4.x
        return ([.. argumentDescriptors], [.. propertyAccessDescriptors]);
    }

    private static void AddAspNetCoreDescriptors(List<ArgumentDescriptor> argumentDescriptors, List<MemberDescriptor> propertyAccessDescriptors)
    {
        argumentDescriptors.AddRange([
            ArgumentDescriptor.ElementAccess(// Request.Form["id"]
                invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                invokedIndexerExpression: "Form",
                parameterConstraint: _ => true, // There is only a single overload and it is getter only
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.Form.TryGetValue("id", out _)
                invokedType: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                methodName: "TryGetValue",
                parameterName: "key",
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.Form.ContainsKey("id")
                invokedType: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                methodName: "ContainsKey",
                parameterName: "key",
                argumentPosition: 0),
            ArgumentDescriptor.ElementAccess(// Request.Headers["id"]
                invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IHeaderDictionary,
                invokedIndexerExpression: "Headers",
                parameterConstraint: IsGetterParameter, // Headers are read/write
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.Headers.TryGetValue("id", out _)
                invokedMethodSymbol: x => IsIDictionaryStringStringValuesInvocation(x, "TryGetValue"), // TryGetValue is from IDictionary<TKey, TValue> here. We check the type arguments.
                invokedMemberNameConstraint: (name, comparison) => string.Equals(name, "TryGetValue", comparison),
                invokedMemberNodeConstraint: IsAccessedViaHeaderDictionary,
                parameterConstraint: x => string.Equals(x.Name, "key", StringComparison.Ordinal),
                argumentListConstraint: (list, position) => list.Count == 2 && position is 0 or null,
                refKind: RefKind.None),
            ArgumentDescriptor.MethodInvocation(// Request.Headers.ContainsKey("id")
                invokedMethodSymbol: x => IsIDictionaryStringStringValuesInvocation(x, "ContainsKey"),
                invokedMemberNameConstraint: (name, comparison) => string.Equals(name, "ContainsKey", comparison),
                invokedMemberNodeConstraint: IsAccessedViaHeaderDictionary,
                parameterConstraint: x => string.Equals(x.Name, "key", StringComparison.Ordinal),
                argumentListConstraint: (list, _) => list.Count == 1,
                refKind: RefKind.None),
            ArgumentDescriptor.ElementAccess(// Request.Query["id"]
                invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IQueryCollection,
                invokedIndexerExpression: "Query",
                parameterConstraint: _ => true, // There is only a single overload and it is getter only
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.Query.TryGetValue("id", out _)
                invokedType: KnownType.Microsoft_AspNetCore_Http_IQueryCollection,
                methodName: "TryGetValue",
                parameterName: "key",
                argumentPosition: 0),
            ArgumentDescriptor.ElementAccess(// Request.RouteValues["id"]
                invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Routing_RouteValueDictionary,
                invokedIndexerExpression: "RouteValues",
                parameterConstraint: IsGetterParameter, // RouteValues are read/write
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.RouteValues.TryGetValue("id", out _)
                invokedType: KnownType.Microsoft_AspNetCore_Routing_RouteValueDictionary,
                methodName: "TryGetValue",
                parameterName: "key",
                argumentPosition: 0)]);

        propertyAccessDescriptors.Add(new(KnownType.Microsoft_AspNetCore_Http_IFormCollection, "Files")); // Request.Form.Files...
    }

    // Check that the "Headers" expression in the Headers.TryGetValue("id", out _) invocation is of type IHeaderDictionary
    private static bool IsAccessedViaHeaderDictionary(SemanticModel model, ILanguageFacade language, SyntaxNode invocation) =>
        invocation is InvocationExpressionSyntax { Expression: { } expression }
            && GetLeftOfDot(expression) is { } left
            && model.GetTypeInfo(left) is { Type: { } typeSymbol } && typeSymbol.Is(KnownType.Microsoft_AspNetCore_Http_IHeaderDictionary);

    private static bool IsOverridingFilterMethods(IMethodSymbol method) =>
        (method.GetOverriddenMember() ?? method).ExplicitOrImplicitInterfaceImplementations().Any(x => x is IMethodSymbol { ContainingType: { } container }
            && container.IsAny(
                KnownType.Microsoft_AspNetCore_Mvc_Filters_IActionFilter,
                KnownType.Microsoft_AspNetCore_Mvc_Filters_IAsyncActionFilter));

    private static bool OriginatesFromParameter(SemanticModel semanticModel, ArgumentSyntax argument) =>
        GetExpressionOfArgumentParent(argument) is { } parentExpression && OriginatesFromParameter(semanticModel, parentExpression);

    private static bool OriginatesFromParameter(SemanticModel semanticModel, ExpressionSyntax expression) =>
        MostLeftOfDottedChain(expression) is { } mostLeft && semanticModel.GetSymbolInfo(mostLeft).Symbol is IParameterSymbol;

    private static ExpressionSyntax GetLeftOfDot(ExpressionSyntax expression) =>
        expression switch
        {
            MemberAccessExpressionSyntax memberAccessExpression => memberAccessExpression.Expression,
            MemberBindingExpressionSyntax memberBindingExpression => memberBindingExpression.GetParentConditionalAccessExpression()?.Expression,
            _ => null,
        };

    private static ExpressionSyntax MostLeftOfDottedChain(ExpressionSyntax root)
    {
        var current = root.GetRootConditionalAccessExpression()?.Expression ?? root;
        while (current.Kind() is SyntaxKind.SimpleMemberAccessExpression or SyntaxKind.ElementAccessExpression)
        {
            current = current switch
            {
                MemberAccessExpressionSyntax { Expression: { } left } => left,
                ElementAccessExpressionSyntax { Expression: { } left } => left,
                _ => throw new InvalidOperationException("Unreachable"),
            };
        }
        return current;
    }

    private static ExpressionSyntax GetExpressionOfArgumentParent(ArgumentSyntax argument) =>
        argument switch
        {
            { Parent: BracketedArgumentListSyntax { Parent: ElementBindingExpressionSyntax expression } } => expression.GetParentConditionalAccessExpression(),
            { Parent: BracketedArgumentListSyntax { Parent: ElementAccessExpressionSyntax { Expression: { } expression } } } => expression,
            { Parent: ArgumentListSyntax { Parent: InvocationExpressionSyntax { Expression: { } expression } } } => expression,
            _ => null,
        };

    private static Location GetPrimaryLocation(ArgumentSyntax argument) =>
        ((SyntaxNode)GetExpressionOfArgumentParent(argument) ?? argument).GetLocation();

    private static bool IsGetterParameter(IParameterSymbol parameter) =>
        parameter.ContainingSymbol is IMethodSymbol { MethodKind: MethodKind.PropertyGet };

    private static bool IsIDictionaryStringStringValuesInvocation(IMethodSymbol method, string name) =>
        method.Is(KnownType.System_Collections_Generic_IDictionary_TKey_TValue, name)
            && method.ContainingType.TypeArguments is { Length: 2 } typeArguments
            && typeArguments[0].Is(KnownType.System_String)
            && typeArguments[1].Is(KnownType.Microsoft_Extensions_Primitives_StringValues);

    private readonly record struct ReportCandidate(string Message, Location Location, bool OriginatesFromParameter = false);
}
