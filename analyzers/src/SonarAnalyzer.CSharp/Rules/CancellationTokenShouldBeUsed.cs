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

// S8949 (this file + .MethodInvocation.cs) and S9426 (.AwaitForEach.cs) both flag a missed opportunity to forward an
// accessible CancellationToken, just at different consumption sites (method calls vs. 'await foreach'). They're merged
// into one analyzer, split via partials per rule, so the "is a CancellationToken in scope for this method" computation
// below - the only part they actually share - runs once per method per compilation instead of twice.
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed partial class CancellationTokenShouldBeUsed : SonarDiagnosticAnalyzer
{
    private readonly record struct CtSource(ExpressionSyntax Expression, bool IsStatic, SyntaxToken? DeclarationToken)
    {
        public CtSource? For(IMethodSymbol method) =>
            !method.IsStatic || IsStatic ? this : null;
    }

    private static readonly ImmutableArray<string> ExcludedHostLifetimeTokens = ImmutableArray.Create("ApplicationStarted", "ApplicationStopped");

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(RuleS8949, RuleS9426);

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
                                        // static CT could be passed but the FN is acceptable, FPs are not. A constructor
                                        // can't be async, so this guard is a no-op for await-foreach-based node kinds.
                                        var initializerSpan = (codeBlockStart.CodeBlock as ConstructorDeclarationSyntax)?.Initializer?.Span;
                                        codeBlockStart.RegisterNodeAction(
                                            nodeContext =>
                                            {
                                                if (initializerSpan?.Contains(nodeContext.Node.Span) != true)
                                                {
                                                    switch (nodeContext.Node.Kind())
                                                    {
                                                        case SyntaxKind.InvocationExpression:
                                                            AnalyzeInvocation(nodeContext, ctSource, owningMethod, candidateCache);
                                                            break;
                                                        case SyntaxKind.ForEachStatement:
                                                        case SyntaxKindEx.ForEachVariableStatement:
                                                            AnalyzeAwaitForEach(nodeContext, ctSource, owningMethod);
                                                            break;
                                                    }
                                                }
                                            },
                                            SyntaxKind.InvocationExpression,
                                            SyntaxKind.ForEachStatement,
                                            SyntaxKindEx.ForEachVariableStatement);
                                    }
                                });
                        },
                        SymbolKind.NamedType);
                }
            });

    /// <summary>
    /// Returns the first field or property of <paramref name="type"/> that is, or wraps (one level deep), a <see cref="CancellationToken"/>.
    /// The <see cref="CtSource.IsStatic"/> flag is preserved so callers can exclude instance sources from static contexts.
    /// Returns <see langword="default"/> when no such member exists.
    /// </summary>
    private static CtSource? FindMemberCtSource(ConcurrentDictionary<ITypeSymbol, IdentifierNameSyntax> ctTypeCache, INamedTypeSymbol type)
    {
        foreach (var member in type.GetMembers())
        {
            if (AsFieldOrProperty(member) is { } fieldOrProp && !IsFixedCancelledToken(fieldOrProp))
            {
                var memberExpr = member.IsStatic
                    ? (ExpressionSyntax)fieldOrProp.Name.EscapedIdentifierName
                    : SyntaxFactory.MemberAccessExpression(SyntaxKind.SimpleMemberAccessExpression, SyntaxFactory.ThisExpression(), fieldOrProp.Name.EscapedIdentifierName);
                if (fieldOrProp.SymbolType is { } memberType
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
    private static CtSource? FindParameterCtSource(ConcurrentDictionary<ITypeSymbol, IdentifierNameSyntax> ctTypeCache, IMethodSymbol method)
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
            if (member is { IsStatic: false, DeclaredAccessibility.IsAccessibleOutsideTheType: true }
                && AsFieldOrProperty(member) is { } fieldOrProp
                && fieldOrProp.SymbolType.Is(KnownType.System_Threading_CancellationToken)
                && !IsExcluded(member))
            {
                return fieldOrProp.Name.EscapedIdentifierName;
            }
        }

        return null;
    }

    // ApplicationStarted/ApplicationStopped are one-shot, already fired (or unreachable) by the time any caller runs.
    // ApplicationStopping is a live gate and remains eligible: https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostapplicationlifetime
    private static bool IsExcluded(ISymbol member) =>
        member.ContainingType.DerivesOrImplements(KnownType.Microsoft_Extensions_Hosting_IHostApplicationLifetime)
        && ExcludedHostLifetimeTokens.Contains(member.Name);

    private static ISymbol AsFieldOrProperty(ISymbol member) =>
        member switch
        {
            IFieldSymbol { IsImplicitlyDeclared: false } => member,
            IPropertySymbol { GetMethod: not null, IsIndexer: false } => member,
            _ => null
        };

    /// <summary>True for a readonly field / get-only property whose sole initializer is <c>new CancellationToken(true)</c> or <c>new(true)</c> —
    /// a fixed, always-cancelled sentinel. Only <see langword="true"/> qualifies; <see langword="false"/> behaves like <see cref="CancellationToken.None"/>.</summary>
    private static bool IsFixedCancelledToken(ISymbol fieldOrProp) =>
        fieldOrProp switch
        {
            IFieldSymbol { IsReadOnly: true } field => IsNewCancellationTokenTrue(SingleInitializer(field)),
            IPropertySymbol { SetMethod: null } property => IsNewCancellationTokenTrue(SingleInitializer(property)),
            _ => false,
        };

    private static ExpressionSyntax SingleInitializer(IFieldSymbol field) =>
        field.FirstSyntaxRef is VariableDeclaratorSyntax { Initializer: { } initializer }
            ? initializer.Value
            : null;

    private static ExpressionSyntax SingleInitializer(IPropertySymbol property) =>
        property.FirstSyntaxRef is PropertyDeclarationSyntax declaration
            ? declaration.Initializer?.Value ?? declaration.ExpressionBody?.Expression
            : null;

    private static bool IsNewCancellationTokenTrue(ExpressionSyntax expression) =>
        ObjectCreationFactory.TryCreate(expression) is { ArgumentList.Arguments: { Count: 1 } arguments }
        && arguments[0].Expression.IsKind(SyntaxKind.TrueLiteralExpression);
}
