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

using System.Collections.Concurrent;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class CancellationTokenShouldBeUsed : SonarDiagnosticAnalyzer
{
    private readonly record struct MethodGroupKey(ITypeSymbol Type, string Name);

    private readonly record struct MemberCtSource(ExpressionSyntax Expression, bool IsStatic, SyntaxToken? DeclarationToken)
    {
        public MemberCtSource? For(IMethodSymbol method) =>
            !method.IsStatic || IsStatic ? this : null;
    }

    private const string DiagnosticId = "S8949";
    private const string MessageFormat = "Pass the '{0}' to this method to allow cancellation of the operation.";
    private const string MessageFormatDefault = "Pass the '{0}' instead of 'default' to allow cancellation of the operation, or use 'CancellationToken.None' to opt out explicitly.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, "{0}");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(
            compilationStart =>
            {
                if (compilationStart.Compilation.GetTypeByMetadataName(KnownType.System_Threading_CancellationToken) is not null)
                {
                    var candidateCache = new ConcurrentDictionary<MethodGroupKey, ImmutableArray<string>>(); // distinct CT parameter names of CT-accepting overloads, per method group
                    var ctTypeCache = new ConcurrentDictionary<ITypeSymbol, IdentifierNameSyntax>(); // CT member name per owner type

                    compilationStart.RegisterSymbolStartAction(
                        symbolStart =>
                        {
                            var memberCtSource = FindMemberCtSource(ctTypeCache, (INamedTypeSymbol)symbolStart.Symbol);

                            symbolStart.RegisterCodeBlockStartAction<SyntaxKind>(
                                codeBlockStart =>
                                {
                                    // IMethodSymbol excludes field and property initializers (OwningSymbol is IFieldSymbol/IPropertySymbol),
                                    // where 'this' is not accessible and instance CT members cannot be referenced.
                                    if (!codeBlockStart.CodeBlock.SyntaxTree.IsGenerated(CSharpGeneratedCodeRecognizer.Instance)
                                        && codeBlockStart.OwningSymbol is IMethodSymbol owningMethod
                                        && (FindParameterCtSource(ctTypeCache, owningMethod) ?? memberCtSource?.For(owningMethod)) is { } ctSource)
                                    {
                                        // Invocations in base()/this() initializer arguments are part of this code block
                                        // but 'this' is not accessible there. Skip them via span intersection (O(1));
                                        // static CT could be passed but the FN is acceptable, FPs are not.
                                        var initializerSpan = (codeBlockStart.CodeBlock as ConstructorDeclarationSyntax)?.Initializer?.Span;
                                        codeBlockStart.RegisterNodeAction(
                                            nodeContext =>
                                            {
                                                if (initializerSpan?.Contains(nodeContext.Node.Span) != true)
                                                {
                                                    Analyze(nodeContext, (InvocationExpressionSyntax)nodeContext.Node, ctSource, candidateCache);
                                                }
                                            },
                                            SyntaxKind.InvocationExpression);
                                    }
                                });
                        },
                        SymbolKind.NamedType);
                }
            });

    /// <summary>
    /// Returns the first field or property of <paramref name="type"/> that is, or wraps (one level deep), a <see cref="CancellationToken"/>.
    /// The <see cref="MemberCtSource.IsStatic"/> flag is preserved so callers can exclude instance sources from static contexts.
    /// Returns <see langword="default"/> when no such member exists.
    /// </summary>
    private static MemberCtSource? FindMemberCtSource(ConcurrentDictionary<ITypeSymbol, IdentifierNameSyntax> ctTypeCache, INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (AsFieldOrProperty(member) is { } fieldOrProp)
            {
                var memberExpr = member.IsStatic
                    ? (ExpressionSyntax)fieldOrProp.Name.EscapedIdentifierName
                    : SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), fieldOrProp.Name.EscapedIdentifierName);
                if (fieldOrProp.GetSymbolType() is { } memberType
                    && (memberType.Is(KnownType.System_Threading_CancellationToken)
                        ? memberExpr
                        : FindOneLevelDeepCtSource(ctTypeCache, memberExpr, memberType)) is { } source)
                {
                    return new(source, member.IsStatic, member.FirstDeclaringReferenceIdentifier);
                }
            }
        }

        return null;
    }

    /// <summary>
    /// Returns a syntax expression referencing the first parameter of <paramref name="method"/> that is, or wraps (one level deep),
    /// a <see cref="CancellationToken"/>. Returns <see langword="null"/> when no such parameter exists.
    /// </summary>
    private static MemberCtSource? FindParameterCtSource(ConcurrentDictionary<ITypeSymbol, IdentifierNameSyntax> ctTypeCache, IMethodSymbol method)
    {
        foreach (var param in method.Parameters)
        {
            if (param.Type.Is(KnownType.System_Threading_CancellationToken))
            {
                return new(param.Name.EscapedIdentifierName, method.IsStatic, param.FirstDeclaringReferenceIdentifier);
            }
            else if (FindOneLevelDeepCtSource(ctTypeCache, param.Name.EscapedIdentifierName, param.Type) is { } deepSource)
            {
                return new(deepSource, method.IsStatic, param.FirstDeclaringReferenceIdentifier);
            }
        }

        return null;
    }

    /// <summary>
    /// If <paramref name="ownerType"/> exposes a public non-static <see cref="CancellationToken"/> member, returns
    /// a member-access expression <c>ownerExpr.Member</c>; otherwise returns <see langword="null"/>.
    /// Results are cached in <paramref name="ctTypeCache"/> (null = no CT member on that type).
    /// </summary>
    private static ExpressionSyntax FindOneLevelDeepCtSource(ConcurrentDictionary<ITypeSymbol, IdentifierNameSyntax> ctTypeCache, ExpressionSyntax ownerExpr, ITypeSymbol ownerType)
    {
        if (ownerType.IsNullableOf(KnownType.System_Threading_CancellationToken))
        {
            return SyntaxFactory.InvocationExpression(
                SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    ownerExpr,
                    SyntaxFactory.IdentifierName(nameof(Nullable<>.GetValueOrDefault))));
        }

        if (ownerType is INamedTypeSymbol { TypeArguments.Length: 1, OriginalDefinition: { } originalDef } named
            && named.TypeArguments[0].Is(KnownType.System_Threading_CancellationToken)
            && originalDef.IsAny(KnownType.System_Threading_AsyncLocal_T, KnownType.System_Threading_ThreadLocal_T))
        {
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                ownerExpr,
                SyntaxFactory.IdentifierName(nameof(AsyncLocal<>.Value)));
        }

        // OriginalDefinition normalizes to the unbound generic (List<int>.OriginalDefinition is List<T>),
        // so ComputeCtMemberName sees T-typed members as T, not CancellationToken — prevents suggesting
        // kvp.Value for KeyValuePair<string, CancellationToken>; all List<T> instantiations share one entry.
        var memberName = ctTypeCache.GetOrAdd(ownerType.OriginalDefinition, ComputeCtMemberName);
        return memberName is null
            ? null
            : SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, ownerExpr, memberName);
    }

    private static IdentifierNameSyntax ComputeCtMemberName(ITypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (!member.IsStatic
                && member.DeclaredAccessibility.IsAccessibleOutsideTheType()
                && AsFieldOrProperty(member) is { } fieldOrProp
                && fieldOrProp.GetSymbolType().Is(KnownType.System_Threading_CancellationToken))
            {
                return fieldOrProp.Name.EscapedIdentifierName;
            }
        }

        return null;
    }

    private static ISymbol AsFieldOrProperty(ISymbol member) =>
        member switch
        {
            IFieldSymbol { IsImplicitlyDeclared: false } => member,
            IPropertySymbol { GetMethod: not null, IsIndexer: false } => member,
            _ => null
        };

    private static void Analyze(
        SonarSyntaxNodeReportingContext nodeContext,
        InvocationExpressionSyntax invocation,
        MemberCtSource ctSource,
        ConcurrentDictionary<MethodGroupKey, ImmutableArray<string>> candidateCache)
    {
        if (nodeContext.Model.GetSymbolInfo(invocation).Symbol is not IMethodSymbol method)
        {
            return;
        }

        // An arg bound to a CT parameter means we are already on the CT overload.
        // Speculative binding would produce a duplicate CT arg and fail; handle directly.
        // ParameterLookup resolves named and reordered args correctly; params CT[] is
        // skipped because its element type (CancellationToken[]) does not match CT.
        var paramLookup = new CSharpMethodParameterLookup(invocation.ArgumentList, method);
        foreach (var arg in invocation.ArgumentList.Arguments)
        {
            if (paramLookup.TryGetSymbol(arg, out var param) && param.Type.NullableUnderlyingTypeOrSelf().Is(KnownType.System_Threading_CancellationToken))
            {
                if (arg.Expression.RawKind is (int)SyntaxKind.DefaultExpression or (int)SyntaxKindEx.DefaultLiteralExpression)
                {
                    nodeContext.ReportIssue(Rule, invocation.Expression, [ctSource.DeclarationToken?.ToSecondaryLocation()], string.Format(MessageFormatDefault, ctSource.Expression));
                }
                return;
            }
        }

        var searchType = (method.ReducedFrom ?? method).ContainingType;
        var ctParamNames = candidateCache.GetOrAdd(new MethodGroupKey(searchType, method.Name), x =>
            x.Type
                .GetMembers(x.Name)
                .OfType<IMethodSymbol>()
                .Select(x => x.Parameters.FirstOrDefault(p => p.Type.NullableUnderlyingTypeOrSelf().Is(KnownType.System_Threading_CancellationToken))?.Name)
                .WhereNotNull()
                .Distinct()
                .ToImmutableArray());

        if (!ctParamNames.IsDefaultOrEmpty && ctParamNames.Any(x => CanSpeculativelyPassCt(nodeContext.Model, invocation, method, x, ctSource.Expression)))
        {
            nodeContext.ReportIssue(Rule, invocation.Expression, [ctSource.DeclarationToken?.ToSecondaryLocation()], string.Format(MessageFormat, ctSource.Expression));
        }
    }

    private static bool CanSpeculativelyPassCt(
        SemanticModel model,
        InvocationExpressionSyntax invocation,
        IMethodSymbol calledMethod,
        string ctParamName,
        ExpressionSyntax ctSource)
    {
        var ctArg = SyntaxFactory.Argument(
            SyntaxFactory.NameColon(ctParamName.EscapedIdentifierName),
            default,
            ctSource);
        var modifiedInvocation = invocation.WithArgumentList(invocation.ArgumentList.AddArguments(ctArg));
        // Roslyn binding resolves symbols without running the diagnostics pass (where static-capture
        // violations such as CS8422 are enforced). So this succeeds even inside a static local function
        // whose caller cannot name ctSource directly — the user still has to add a CT parameter there.
        // GetSpeculativeSymbolInfo resolves by argument-type matching only; it does not check whether
        // the resolved method's return type is compatible with how the result is used at the call site.
        // Check return type on the resolved symbol to avoid false positives (e.g. Task<int> vs Task<string> overloads).
        return model.GetSpeculativeSymbolInfo(invocation.SpanStart, modifiedInvocation, SpeculativeBindingOption.BindAsExpression).Symbol
            is IMethodSymbol resolved
            && model.Compilation.ClassifyConversion(resolved.ReturnType, calledMethod.ReturnType).IsImplicit;
    }
}
