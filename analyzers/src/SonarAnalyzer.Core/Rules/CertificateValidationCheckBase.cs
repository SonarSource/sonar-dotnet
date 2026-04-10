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

namespace SonarAnalyzer.Core.Rules;

public abstract class CertificateValidationCheckBase<
    TSyntaxKind,
    TArgumentSyntax,
    TExpressionSyntax,
    TIdentifierNameSyntax,
    TAssignmentExpressionSyntax,
    TInvocationExpressionSyntax,
    TParameterSyntax,
    TVariableSyntax,
    TLambdaSyntax,
    TMemberAccessSyntax
    > : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TArgumentSyntax : SyntaxNode
    where TExpressionSyntax : SyntaxNode
    where TIdentifierNameSyntax : SyntaxNode
    where TAssignmentExpressionSyntax : SyntaxNode
    where TInvocationExpressionSyntax : SyntaxNode
    where TParameterSyntax : SyntaxNode
    where TVariableSyntax : SyntaxNode
    where TLambdaSyntax : SyntaxNode
    where TMemberAccessSyntax : SyntaxNode
{
    private const string SecondaryMessage = "This function trusts all certificates.";
    private const string DiagnosticId = "S4830";

    internal /* for testing */ abstract MethodParameterLookupBase<TArgumentSyntax> CreateParameterLookup(SyntaxNode argumentListNode, IMethodSymbol method);
    protected abstract HashSet<TSyntaxKind> MethodDeclarationKinds { get; }
    protected abstract HashSet<TSyntaxKind> TypeDeclarationKinds { get; }
    protected abstract Location ExpressionLocation(SyntaxNode expression);
    protected abstract void SplitAssignment(TAssignmentExpressionSyntax assignment, out TIdentifierNameSyntax leftIdentifier, out TExpressionSyntax right);
    protected abstract IEqualityComparer<TExpressionSyntax> CreateNodeEqualityComparer();
    protected abstract TExpressionSyntax[] FindReturnAndThrowExpressions(InspectionContext c, SyntaxNode block);
    protected abstract bool IsTrueLiteral(TExpressionSyntax expression);
    protected abstract TExpressionSyntax VariableInitializer(TVariableSyntax variable);
    protected abstract ImmutableArray<Location> LambdaLocations(InspectionContext c, TLambdaSyntax lambda);
    protected abstract SyntaxNode LocalVariableScope(TVariableSyntax variable);
    protected abstract SyntaxNode ExtractArgumentExpressionNode(SyntaxNode expression);
    protected abstract SyntaxNode SyntaxFromReference(SyntaxReference reference);

    protected override string MessageFormat => "Enable server certificate validation on this SSL/TLS connection";

    protected CertificateValidationCheckBase() : base(DiagnosticId) { }

    protected void CheckAssignmentSyntax(SonarSyntaxNodeReportingContext c)
    {
        SplitAssignment((TAssignmentExpressionSyntax)c.Node, out var leftIdentifier, out var right);
        if (leftIdentifier is not null && right is not null
            && c.Model.GetSymbolInfo(leftIdentifier).Symbol is IPropertySymbol left
            && IsValidationDelegateType(left.Type))
        {
            TryReportLocations(new InspectionContext(c), leftIdentifier.GetLocation(), right);
        }
    }

    protected void CheckConstructorParameterSyntax(SonarSyntaxNodeReportingContext c)
    {
        if (c.Model.GetSymbolInfo(c.Node).Symbol is IMethodSymbol ctor)
        {
            MethodParameterLookupBase<TArgumentSyntax> methodParamLookup = null;       // Cache, there might be more of them
            // Validation for TryGetNonParamsSyntax, ParamArray/params and therefore array arguments are not inspected
            foreach (var param in ctor.Parameters.Where(x => !x.IsParams && IsValidationDelegateType(x.Type)))
            {
                methodParamLookup ??= CreateParameterLookup(c.Node, ctor);
                if (methodParamLookup.TryGetNonParamsSyntax(param, out var expression))
                {
                    TryReportLocations(new InspectionContext(c), ExpressionLocation(expression), expression);
                }
            }
        }
    }

    protected ImmutableArray<Location> ParamLocations(InspectionContext c, TParameterSyntax param)
    {
        var containingMethodDeclaration = param.FirstAncestorOrSelf((SyntaxNode x) => Language.Syntax.IsAnyKind(x, MethodDeclarationKinds));
        if (containingMethodDeclaration is null || !c.VisitedMethods.Add(containingMethodDeclaration))
        {
            return ImmutableArray<Location>.Empty;
        }
        // Validation for TryGetNonParamsSyntax, ParamArray/params and therefore array arguments are not inspected
        var paramSymbol = containingMethodDeclaration.EnsureCorrectSemanticModelOrDefault(c.Context.Model)?.GetDeclaredSymbol(containingMethodDeclaration) is IMethodSymbol containingMethod
                            && Language.Syntax.NodeIdentifier(param)?.ValueText is { } identText
                                ? containingMethod.Parameters.Single(x => x.Name == identText)
                                : null;
        if (paramSymbol is null || paramSymbol.IsParams)
        {
            return ImmutableArray<Location>.Empty;
        }
        var methodSymbol = paramSymbol.ContainingSymbol as IMethodSymbol;
        return FindInvocationList(c.Context, FindRootTypeDeclaration(param), methodSymbol)
            .SelectMany(x => CreateParameterLookup(x, methodSymbol).TryGetNonParamsSyntax(paramSymbol, out var expr) && expr is not null
                                ? CallStackSublocations(c, expr)
                                : Enumerable.Empty<Location>())
            .ToImmutableArray();
    }

    protected ImmutableArray<Location> BlockLocations(InspectionContext c, SyntaxNode block)
    {
        var ret = ImmutableArray.CreateBuilder<Location>();
        if (block is not null)
        {
            var returnExpressions = FindReturnAndThrowExpressions(c, block);
            // There must be at least one return, that does not return true to be compliant. There can be NULL from standalone Throw statement.
            if (returnExpressions.All(IsTrueLiteral))
            {
                ret.AddRange(returnExpressions.Select(x => x.GetLocation()));
            }
        }
        return ret.ToImmutable();
    }

    protected virtual SyntaxNode FindRootTypeDeclaration(SyntaxNode node)
    {
        SyntaxNode candidate;
        var current = node.FirstAncestorOrSelf<SyntaxNode>(IsTypeDeclaration);
        while (current is not null && (candidate = current.Parent.FirstAncestorOrSelf<SyntaxNode>(IsTypeDeclaration)) is not null)  // Search for parent of nested type
        {
            current = candidate;
        }
        return current;
    }

    private bool IsTypeDeclaration(SyntaxNode node) =>
        Language.Syntax.IsAnyKind(node, TypeDeclarationKinds);

    private void TryReportLocations(InspectionContext c, Location primaryLocation, SyntaxNode expression)
    {
        var locations = ArgumentLocations(c, expression);
        if (!locations.IsEmpty)
        {
            // Report both, assignment as well as all implementation occurrences
            c.Context.ReportIssue(Rule, primaryLocation, locations.ToSecondary(SecondaryMessage));
        }
    }

    private static bool IsValidationDelegateType(ITypeSymbol type)
    {
        if (type.Is(KnownType.System_Net_Security_RemoteCertificateValidationCallback))
        {
            return true;
        }
        if (type is INamedTypeSymbol namedSymbol && type.OriginalDefinition.Is(KnownType.System_Func_T1_T2_T3_T4_TResult))
        {
            // HttpClientHandler.ServerCertificateCustomValidationCallback uses Func<HttpRequestMessage, X509Certificate2, X509Chain, SslPolicyErrors, bool>
            // We're actually looking for Func<Any Sender, X509Certificate2, X509Chain, SslPolicyErrors, bool>
            var parameters = namedSymbol.DelegateInvokeMethod.Parameters;
            return parameters.Length == 4   // And it should! T1, T2, T3, T4
                && parameters[0].Type.IsClassOrStruct() // We don't care about common (Object) nor specific (HttpRequestMessage) type of Sender
                && parameters[1].Type.Is(KnownType.System_Security_Cryptography_X509Certificates_X509Certificate2)
                && parameters[2].Type.Is(KnownType.System_Security_Cryptography_X509Certificates_X509Chain)
                && parameters[3].Type.Is(KnownType.System_Net_Security_SslPolicyErrors)
                && namedSymbol.DelegateInvokeMethod.ReturnType.Is(KnownType.System_Boolean);
        }
        return false;
    }

    private ImmutableArray<Location> ArgumentLocations(InspectionContext c, SyntaxNode expression)
    {
        switch (ExtractArgumentExpressionNode(expression))
        {
            case TIdentifierNameSyntax identifier:
                if (SymbolFromCorrectModel(identifier, c.Context.Model) is { DeclaringSyntaxReferences.Length: 1 } identSymbol)
                {
                    return IdentifierLocations(c, SyntaxFromReference(identSymbol.DeclaringSyntaxReferences.Single()));
                }
                break;
            case TLambdaSyntax lambda:
                return LambdaLocations(c, lambda);
            case TInvocationExpressionSyntax invocation:
                return VisitInvocation(invocation, c);
            case TMemberAccessSyntax memberAccess:
                if (SymbolFromCorrectModel(memberAccess, c.Context.Model) is { } maSymbol
                    && maSymbol.IsInType(KnownType.System_Net_Http_HttpClientHandler)
                    && maSymbol.Name == "DangerousAcceptAnyServerCertificateValidator")
                {
                    return new[] { memberAccess.GetLocation() }.ToImmutableArray();
                }
                break;
        }
        return ImmutableArray<Location>.Empty;
    }

    private ImmutableArray<Location> VisitInvocation(TInvocationExpressionSyntax invocation, InspectionContext c)
    {
        if (SymbolFromCorrectModel(invocation, c.Context.Model) is IMethodSymbol invSymbol
            && (invSymbol.PartialImplementationPart?.DeclaringSyntaxReferences ?? invSymbol.DeclaringSyntaxReferences).SingleOrDefault() is { } reference)
        {
            var syntax = SyntaxFromReference(reference);
            c.VisitedMethods.Add(syntax);
            return InvocationLocations(c, syntax);
        }
        return ImmutableArray<Location>.Empty;
    }

    private ImmutableArray<Location> IdentifierLocations(InspectionContext c, SyntaxNode syntax) =>
        syntax switch
        {
            TParameterSyntax parameter => ParamLocations(c, parameter), // Value arrived as a parameter
            TVariableSyntax variable => VariableLocations(c, variable), // Value passed as variable
            _ => Language.Syntax.IsAnyKind(syntax, MethodDeclarationKinds)
                ? BlockLocations(c, syntax)                             // Direct delegate name
                : ImmutableArray<Location>.Empty,
        };

    private ImmutableArray<Location> VariableLocations(InspectionContext c, TVariableSyntax variable)
    {
        var allAssignedExpressions = new List<TExpressionSyntax>();
        var parentScope = LocalVariableScope(variable);
        if (parentScope is not null)
        {
            var identText = Language.Syntax.NodeIdentifier(variable)?.ValueText;
            allAssignedExpressions.AddRange(parentScope.DescendantNodes().OfType<TAssignmentExpressionSyntax>()
                .Select(x =>
                    {
                        SplitAssignment(x, out var leftIdentifier, out var right);
                        return new { leftIdentifier, right };
                    })
                .Where(x => x.leftIdentifier is not null && Language.Syntax.NodeIdentifier(x.leftIdentifier) is { } identifier && identifier.ValueText == identText)
                .Select(x => x.right));
        }
        var initializer = VariableInitializer(variable);
        if (initializer is not null)       // Declarator initializer is counted as (default) assignment as well
        {
            allAssignedExpressions.Add(initializer);
        }
        return MultiExpressionSublocations(c, allAssignedExpressions);
    }

    private ImmutableArray<Location> InvocationLocations(InspectionContext c, SyntaxNode method)
    {
        // Ignore all return statements with recursive call. Result depends on returns that could return compliant validator.
        var returnExpressionSublocationsList = FindReturnAndThrowExpressions(c, method).Where(x => !IsVisited(c, x));
        return MultiExpressionSublocations(c, returnExpressionSublocationsList);
    }

    private bool IsVisited(InspectionContext c, SyntaxNode expression) =>
        expression is TInvocationExpressionSyntax invocation
        && SymbolFromCorrectModel(invocation, c.Context.Model) is { } symbol
        && symbol.DeclaringSyntaxReferences.Select(SyntaxFromReference).Any(c.VisitedMethods.Contains);

    private ImmutableArray<Location> MultiExpressionSublocations(InspectionContext c, IEnumerable<TExpressionSyntax> expressions)
    {
        var exprSublocationsList = expressions.Distinct(CreateNodeEqualityComparer())
            .Select(x => CallStackSublocations(c, x))
            .ToArray();
        // If there's at least one concurrent expression, that returns compliant delegate, then there's some logic and this scope is compliant
        if (exprSublocationsList.Any(x => x.IsEmpty))
        {
            return ImmutableArray<Location>.Empty;
        }
        return exprSublocationsList.SelectMany(x => x).ToImmutableArray();      // Else every return statement is noncompliant
    }

    private ImmutableArray<Location> CallStackSublocations(InspectionContext c, SyntaxNode expression)
    {
        var lst = ArgumentLocations(c, expression);
        if (!lst.IsEmpty)        // There's noncompliant issue in this chain
        {
            var loc = expression.GetLocation();
            if (!lst.Any(x => x.SourceSpan.IntersectsWith(loc.SourceSpan)))
            {
                // Add 2nd, 3rd, 4th etc //Secondary marker. If it is not marked already from direct Delegate name or direct Lambda occurrence
                return lst.Concat([loc]).ToImmutableArray();
            }
        }
        return lst;
    }

    private ImmutableArray<TInvocationExpressionSyntax> FindInvocationList(SonarSyntaxNodeReportingContext c, SyntaxNode root, IMethodSymbol method)
    {
        if (root is null || method is null)
        {
            return ImmutableArray<TInvocationExpressionSyntax>.Empty;
        }
        var ret = ImmutableArray.CreateBuilder<TInvocationExpressionSyntax>();
        foreach (var invocation in root.DescendantNodesAndSelf().OfType<TInvocationExpressionSyntax>())
        {
            if (Language.Syntax.InvocationIdentifier(invocation) is { } invocationIdentifier
                && invocationIdentifier.ValueText.Equals(method.Name, Language.NameComparison)
                && SymbolFromCorrectModel(invocation, c.Model) is { } symbol
                && symbol.Equals(method))
            {
                ret.Add(invocation);
            }
        }
        return ret.ToImmutable();
    }

    private static ISymbol SymbolFromCorrectModel(SyntaxNode node, SemanticModel model) =>
        node.EnsureCorrectSemanticModelOrDefault(model) is { } correctModel
        && correctModel.GetSymbolInfo(node).Symbol is { } symbol
            ? symbol
            : null;

    protected readonly struct InspectionContext
    {
        public readonly SonarSyntaxNodeReportingContext Context;
        public readonly HashSet<SyntaxNode> VisitedMethods;

        public InspectionContext(SonarSyntaxNodeReportingContext context)
        {
            Context = context;
            VisitedMethods = [];
        }
    }
}
