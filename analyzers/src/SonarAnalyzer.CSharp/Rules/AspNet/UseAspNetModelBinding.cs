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

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStartContext =>
        {
            var descriptors = GetDescriptors(compilationStartContext.Compilation);
            if (descriptors.ArgumentDescriptors.Any() || descriptors.PropertyAccessDescriptors.Any())
            {
                SymbolRegistrations(compilationStartContext, descriptors);
            }
        });

    private void SymbolRegistrations(SonarCompilationStartAnalysisContext compilationStartContext, Descriptors descriptors) =>
        compilationStartContext.RegisterSymbolStartAction(symbolStart =>
        {
            // If the user overrides any action filters, model binding may not be working as expected.
            // Then we do not want to raise on expressions that originate from parameters.
            // See the OverridesController.Undecidable test cases for details.
            var hasActionFiltersOverrides = false;
            var candidates = new ConcurrentStack<ReportCandidate>(); // In SymbolEnd, we filter the candidates based on the overriding we learn on the go.
            if (((INamedTypeSymbol)symbolStart.Symbol).IsControllerType())
            {
                symbolStart.RegisterCodeBlockStartAction<SyntaxKind>(codeBlockStart =>
                {
                    if (IsOverridingFilterMethods(codeBlockStart.OwningSymbol))
                    {
                        // We do not want to raise in ActionFilter overrides and so we do not register.
                        // The SymbolEndAction needs to be made aware, that there are
                        // ActionFilter overrides, so it can filter out some candidates.
                        hasActionFiltersOverrides = true;
                    }
                    else
                    {
                        RegisterCodeBlockActions(codeBlockStart, descriptors, candidates);
                    }
                });
            }
            symbolStart.RegisterSymbolEndAction(symbolEnd =>
            {
                foreach (var candidate in candidates.Where(x => !(hasActionFiltersOverrides && x.OriginatesFromParameter)))
                {
                    symbolEnd.ReportIssue(Diagnostic.Create(Rule, candidate.Location, candidate.Message));
                }
            });
        }, SymbolKind.NamedType);

    private void RegisterCodeBlockActions(
        SonarCodeBlockStartAnalysisContext<SyntaxKind> codeBlockStart,
        Descriptors descriptors,
        ConcurrentStack<ReportCandidate> controllerCandidates)
    {
        // Within a single code block, access via constant and variable keys could be mixed.
        // We only want to raise, if all access were done via constants.
        var allConstantAccesses = true;
        var codeBlockCandidates = new ConcurrentStack<ReportCandidate>();
        var (argumentDescriptors, propertyAccessDescriptors) = descriptors;
        if (argumentDescriptors.Any())
        {
            codeBlockStart.RegisterNodeAction(nodeContext =>
            {
                var argument = (ArgumentSyntax)nodeContext.Node;
                var model = nodeContext.SemanticModel;
                if (allConstantAccesses
                    && AddMatchingArgumentToCandidates(model, codeBlockCandidates, argument, argumentDescriptors)
                    && model.GetConstantValue(argument.Expression) is not { HasValue: true, Value: string })
                {
                    allConstantAccesses = false;
                }
            }, SyntaxKind.Argument);
        }
        if (propertyAccessDescriptors.Any())
        {
            codeBlockStart.RegisterNodeAction(nodeContext =>
            {
                // The property access of Request.Form.Files can be replaced by an IFormFile binding.
                // Any access to a "Files" property is therefore noncompliant. This is different from the Argument handling above.
                var memberAccess = (MemberAccessExpressionSyntax)nodeContext.Node;
                var context = new PropertyAccessContext(memberAccess, nodeContext.SemanticModel, memberAccess.Name.Identifier.ValueText);
                if (Language.Tracker.PropertyAccess.MatchProperty(propertyAccessDescriptors)(context))
                {
                    codeBlockCandidates.Push(new(UseIFormFileBindingMessage, memberAccess.GetLocation(), IsOriginatingFromParameter(nodeContext.SemanticModel, memberAccess)));
                }
            }, SyntaxKind.SimpleMemberAccessExpression);
        }
        codeBlockStart.RegisterCodeBlockEndAction(codeBlockEnd =>
        {
            if (allConstantAccesses
                && codeBlockCandidates.ToArray() is { Length: > 0 } candidates) // Net core 2.2: PushRange throws for empty arrays https://stackoverflow.com/q/7487097
            {
                controllerCandidates.PushRange(candidates);
            }
        });
    }

    private bool AddMatchingArgumentToCandidates(
        SemanticModel model,
        ConcurrentStack<ReportCandidate> codeBlockCandidates,
        ArgumentSyntax argument,
        ArgumentDescriptor[] argumentDescriptors)
    {
        var context = new ArgumentContext(argument, model);
        if (Array.Exists(argumentDescriptors, x => Language.Tracker.Argument.MatchArgument(x)(context)))
        {
            codeBlockCandidates.Push(new(UseAspNetModelBindingMessage, GetPrimaryLocation(argument), IsOriginatingFromParameter(model, argument)));
            return true;
        }
        return false;
    }

    private static Descriptors GetDescriptors(Compilation compilation)
    {
        var argumentDescriptors = new List<ArgumentDescriptor>();
        var propertyAccessDescriptors = new List<MemberDescriptor>();
        if (compilation.GetTypeByMetadataName(KnownType.Microsoft_AspNetCore_Mvc_ControllerAttribute) is { })
        {
            AddAspNetCoreDescriptors(argumentDescriptors, propertyAccessDescriptors);
        }
        // TODO: Add descriptors for Asp.Net MVC 4.x
        return new([.. argumentDescriptors], [.. propertyAccessDescriptors]);
    }

    private static void AddAspNetCoreDescriptors(List<ArgumentDescriptor> argumentDescriptors, List<MemberDescriptor> propertyAccessDescriptors)
    {
        const string TryGetValue = nameof(IDictionary<int, int>.TryGetValue);
        const string ContainsKey = nameof(IDictionary<int, int>.ContainsKey);
        argumentDescriptors.AddRange([
            ArgumentDescriptor.ElementAccess(// Request.Form["id"]
                invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                invokedIndexerExpression: "Form",
                parameterConstraint: _ => true, // There is only a single overload and it is getter only
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.Form.TryGetValue("id", out _)
                invokedType: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                methodName: TryGetValue,
                parameterName: "key",
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.Form.ContainsKey("id")
                invokedType: KnownType.Microsoft_AspNetCore_Http_IFormCollection,
                methodName: ContainsKey,
                parameterName: "key",
                argumentPosition: 0),
            ArgumentDescriptor.ElementAccess(// Request.Headers["id"]
                invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Http_IHeaderDictionary,
                invokedIndexerExpression: "Headers",
                parameterConstraint: IsGetterParameter, // Headers are read/write
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.Headers.TryGetValue("id", out _)
                invokedMemberConstraint: x => IsIDictionaryStringStringValuesInvocation(x, TryGetValue), // TryGetValue is from IDictionary<TKey, TValue> here. We check the type arguments.
                invokedMemberNameConstraint: (name, comparison) => string.Equals(name, TryGetValue, comparison),
                invokedMemberNodeConstraint: IsAccessedViaHeaderDictionary,
                parameterConstraint: x => string.Equals(x.Name, "key", StringComparison.Ordinal),
                argumentListConstraint: (list, position) => list.Count == 2 && position is 0 or null,
                refKind: RefKind.None),
            ArgumentDescriptor.MethodInvocation(// Request.Headers.ContainsKey("id")
                invokedMemberConstraint: x => IsIDictionaryStringStringValuesInvocation(x, ContainsKey),
                invokedMemberNameConstraint: (name, comparison) => string.Equals(name, ContainsKey, comparison),
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
                methodName: TryGetValue,
                parameterName: "key",
                argumentPosition: 0),
            ArgumentDescriptor.ElementAccess(// Request.RouteValues["id"]
                invokedIndexerContainer: KnownType.Microsoft_AspNetCore_Routing_RouteValueDictionary,
                invokedIndexerExpression: "RouteValues",
                parameterConstraint: IsGetterParameter, // RouteValues are read/write
                argumentPosition: 0),
            ArgumentDescriptor.MethodInvocation(// Request.RouteValues.TryGetValue("id", out _)
                invokedType: KnownType.Microsoft_AspNetCore_Routing_RouteValueDictionary,
                methodName: TryGetValue,
                parameterName: "key",
                argumentPosition: 0)]);

        propertyAccessDescriptors.Add(new(KnownType.Microsoft_AspNetCore_Http_IFormCollection, "Files")); // Request.Form.Files...
    }

    // Check that the "Headers" expression in the Headers.TryGetValue("id", out _) invocation is of type IHeaderDictionary
    private static bool IsAccessedViaHeaderDictionary(SemanticModel model, ILanguageFacade language, SyntaxNode invocation) =>
        invocation is InvocationExpressionSyntax { Expression: { } expression }
            && GetLeftOfDot(expression) is { } left
            && model.GetTypeInfo(left) is { Type: { } typeSymbol } && typeSymbol.Is(KnownType.Microsoft_AspNetCore_Http_IHeaderDictionary);

    private static bool IsOverridingFilterMethods(ISymbol owningSymbol) =>
        (owningSymbol.GetOverriddenMember() ?? owningSymbol).ExplicitOrImplicitInterfaceImplementations().Any(x => x is IMethodSymbol { ContainingType: { } container }
            && container.IsAny(
                KnownType.Microsoft_AspNetCore_Mvc_Filters_IActionFilter,
                KnownType.Microsoft_AspNetCore_Mvc_Filters_IAsyncActionFilter));

    private static bool IsOriginatingFromParameter(SemanticModel semanticModel, ArgumentSyntax argument) =>
        GetExpressionOfArgumentParent(argument) is { } parentExpression && IsOriginatingFromParameter(semanticModel, parentExpression);

    private static bool IsOriginatingFromParameter(SemanticModel semanticModel, ExpressionSyntax expression) =>
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

    private readonly record struct ReportCandidate(string Message, Location Location, bool OriginatesFromParameter);
    private readonly record struct Descriptors(ArgumentDescriptor[] ArgumentDescriptors, MemberDescriptor[] PropertyAccessDescriptors);
}
