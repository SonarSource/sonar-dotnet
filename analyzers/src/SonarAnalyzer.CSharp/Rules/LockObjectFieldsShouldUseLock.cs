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
public sealed class LockObjectFieldsShouldUseLock : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S9422";
    private const string MessageFormat = "Use 'System.Threading.Lock' for field '{0}'.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterCompilationStartAction(compilationStart =>
            {
                if (compilationStart.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp13)
                    && compilationStart.Compilation.GetTypeByMetadataName(KnownType.System_Threading_Lock) is { DeclaredAccessibility: Accessibility.Public } lockType
                    && lockType.GetTypeMembers("Scope").Any(x => x is { TypeKind: TypeKind.Struct, IsRefLikeType: true, DeclaredAccessibility: Accessibility.Public }))
                {
                    compilationStart.RegisterSymbolStartAction(symbolStart =>
                        {
                            if (symbolStart.Symbol is INamedTypeSymbol { TypeKind: TypeKind.Class or TypeKind.Struct } containingType)
                            {
                                RegisterFieldAnalysis(symbolStart, containingType);
                            }
                        },
                        SymbolKind.NamedType);
                }
            });

    private static void RegisterFieldAnalysis(SonarSymbolStartAnalysisContext symbolStart, INamedTypeSymbol containingType)
    {
        var candidates = containingType.GetMembers().OfType<IFieldSymbol>()
            .Where(x => x.Type.Is(KnownType.System_Object) && x.DeclaredAccessibility == Accessibility.Private)
            .Where(x => x.DeclaringSyntaxReferences.Length == 1
                        && x.DeclaringSyntaxReferences[0].GetSyntax(symbolStart.Cancel) is VariableDeclaratorSyntax { Parent: VariableDeclarationSyntax { Variables.Count: 1 } })
            .ToHashSet();
        if (candidates.Count == 0)
        {
            return;
        }

        var usages = new FieldUsages(candidates);
        symbolStart.RegisterSyntaxNodeAction(nodeContext => usages.Track((IdentifierNameSyntax)nodeContext.Node, nodeContext.Model, nodeContext.Cancel), SyntaxKind.IdentifierName);
        symbolStart.RegisterSymbolEndAction(symbolEnd =>
            {
                // Private fields are also accessible from nested types, whose syntax is not visited by the actions registered above.
                if (usages.HasReportableFields)
                {
                    foreach (var declaration in containingType.GetTypeMembers().SelectMany(x => x.DeclaringSyntaxReferences).Select(x => x.GetSyntax(symbolEnd.Cancel)))
                    {
                        var model = symbolEnd.Compilation.GetSemanticModel(declaration.SyntaxTree);
                        foreach (var identifier in declaration.DescendantNodes().OfType<IdentifierNameSyntax>())
                        {
                            usages.Track(identifier, model, symbolEnd.Cancel);
                        }
                    }
                }
                ReportIssues(symbolEnd, usages);
            });
    }

    private static void ReportIssues(SonarSymbolReportingContext context, FieldUsages usages)
    {
        foreach (var usage in usages.LockUsages)
        {
            if (!usages.UnsafeFields.ContainsKey(usage.Key)
                && usage.Key.DeclaringSyntaxReferences[0].GetSyntax(context.Cancel) is VariableDeclaratorSyntax declarator
                && (declarator.Initializer is null || IsObjectCreation(declarator.Initializer.Value, context.Compilation.GetSemanticModel(declarator.SyntaxTree))))
            {
                context.ReportIssue(CSharpGeneratedCodeRecognizer.Instance, Rule, declarator.Identifier.GetLocation(), usage.Value.OrderBy(x => x.Location.SourceSpan.Start), usage.Key.Name);
            }
        }
    }

    private static bool IsObjectCreation(ExpressionSyntax expression, SemanticModel model) =>
        ObjectCreationFactory.TryCreate(expression.WithoutEnclosingParentheses, out var objectCreation)
        && objectCreation.TypeSymbol(model).Is(KnownType.System_Object);

    // A field can only be safely converted when every reference is a lock operand, an assignment of a new object, or a nameof operand.
    private sealed class FieldUsages
    {
        private readonly HashSet<IFieldSymbol> candidates;
        private readonly HashSet<string> candidateNames;

        public ConcurrentDictionary<IFieldSymbol, ConcurrentBag<SecondaryLocation>> LockUsages { get; } = new();
        public ConcurrentDictionary<IFieldSymbol, byte> UnsafeFields { get; } = new();
        public bool HasReportableFields => candidates.Any(x => !UnsafeFields.ContainsKey(x));

        public FieldUsages(HashSet<IFieldSymbol> candidates)
        {
            this.candidates = candidates;
            candidateNames = candidates.Select(x => x.Name).ToHashSet();
        }

        public void Track(IdentifierNameSyntax identifier, SemanticModel model, CancellationToken cancel)
        {
            if (candidateNames.Contains(identifier.Identifier.ValueText)
                && model.GetSymbolInfo(identifier, cancel).Symbol is IFieldSymbol field
                && candidates.Contains(field.OriginalDefinition))
            {
                TrackField(identifier, field.OriginalDefinition, model);
            }
        }

        private void TrackField(IdentifierNameSyntax identifier, IFieldSymbol field, SemanticModel model)
        {
            var reference = identifier.Parent switch
            {
                MemberAccessExpressionSyntax memberAccess when memberAccess.Name == identifier => memberAccess.SelfOrTopParenthesizedExpression,
                MemberBindingExpressionSyntax memberBinding when memberBinding.Name == identifier => memberBinding.GetRootConditionalAccessExpression().SelfOrTopParenthesizedExpression,
                _ => identifier.SelfOrTopParenthesizedExpression
            };

            if (reference.Parent is LockStatementSyntax lockStatement && lockStatement.Expression == reference)
            {
                if (ContainsYield(lockStatement) || IsInAsyncFunction(lockStatement, model))
                {
                    UnsafeFields.TryAdd(field, 0);
                }
                else
                {
                    LockUsages.GetOrAdd(field, _ => []).Add(lockStatement.Expression.ToSecondaryLocation());
                }
            }
            else if (!IsObjectAssignment(reference, model) && !reference.IsInNameOfArgument(model))
            {
                UnsafeFields.TryAdd(field, 0);
            }
        }

        // Lock.Scope is a ref struct: it cannot survive a yield of the iterator that directly contains the lock statement (nested functions have their own state machine).
        private static bool ContainsYield(LockStatementSyntax lockStatement) =>
            lockStatement.Statement.DescendantNodesAndSelf(x => !x.IsKind(SyntaxKindEx.LocalFunctionStatement) && x is not AnonymousFunctionExpressionSyntax)
                .OfType<YieldStatementSyntax>()
                .Any();

        // CS9217: a lock on System.Threading.Lock is not allowed anywhere in an async method, local function or lambda, even without await inside the lock body.
        private static bool IsInAsyncFunction(LockStatementSyntax lockStatement, SemanticModel model) =>
            model.GetEnclosingSymbol(lockStatement.SpanStart) is IMethodSymbol { IsAsync: true };

        private static bool IsObjectAssignment(SyntaxNode reference, SemanticModel model) =>
            reference.Parent is AssignmentExpressionSyntax { RawKind: (int)SyntaxKind.SimpleAssignmentExpression } assignment
            && assignment.Left == reference
            && IsObjectCreation(assignment.Right, model);
    }
}
