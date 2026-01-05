/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

using MemberUsage = SonarAnalyzer.Core.Common.NodeSymbolAndModel<Microsoft.CodeAnalysis.CSharp.Syntax.SimpleNameSyntax, Microsoft.CodeAnalysis.ISymbol>;

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class NotAssignedPrivateMember : SonarDiagnosticAnalyzer
{
    /*
     CS0649 reports the same on internal fields. So that's wider in scope, but that's not a live Roslyn analyzer,
     the issue only shows up at build time and not during editing.
    */

    private const string DiagnosticId = "S3459";
    private const string MessageFormat = "Remove unassigned {0} '{1}', or set its value.";
    private const Accessibility MaxAccessibility = Accessibility.Private;

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly HashSet<SyntaxKind> PreOrPostfixOpSyntaxKinds =
    [
        SyntaxKind.PostDecrementExpression,
        SyntaxKind.PostIncrementExpression,
        SyntaxKind.PreDecrementExpression,
        SyntaxKind.PreIncrementExpression,
    ];

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSymbolAction(
            c =>
            {
                var namedType = (INamedTypeSymbol)c.Symbol;
                if (TypeDefinitionShouldBeAnalyzed(namedType))
                {
                    var removableDeclarationCollector = new CSharpRemovableDeclarationCollector(namedType, c.Compilation);
                    var allCandidateMembers = CandidateDeclarations(removableDeclarationCollector);
                    if (allCandidateMembers.Any())
                    {
                        var usedMembers = MemberUsages(removableDeclarationCollector, allCandidateMembers.Select(t => t.Symbol).ToHashSet());
                        var usedMemberSymbols = usedMembers.Select(x => x.Symbol).ToHashSet();
                        var unassignedUsedMemberSymbols = allCandidateMembers.Where(x => usedMemberSymbols.Contains(x.Symbol) && !AssignedMemberSymbols(usedMembers).Contains(x.Symbol));
                        foreach (var candidateMember in unassignedUsedMemberSymbols)
                        {
                            c.ReportIssue(
                                Rule,
                                candidateMember.Node.GetIdentifier().Value.GetLocation(),
                                candidateMember.Node is VariableDeclaratorSyntax ? "field" : "auto-property",
                                candidateMember.Symbol.Name);
                        }
                    }
                }
            },
            SymbolKind.NamedType);

    private static List<NodeSymbolAndModel<SyntaxNode, ISymbol>> CandidateDeclarations(CSharpRemovableDeclarationCollector removableDeclarationCollector)
    {
        var candidateFields = removableDeclarationCollector.RemovableFieldLikeDeclarations(new HashSet<SyntaxKind> { SyntaxKind.FieldDeclaration }, MaxAccessibility)
            .Where(x => !IsInitializedOrFixed((VariableDeclaratorSyntax)x.Node)
                        && !HasStructLayoutAttribute(x.Symbol.ContainingType));
        var candidateProperties = removableDeclarationCollector.RemovableDeclarations(new HashSet<SyntaxKind> { SyntaxKind.PropertyDeclaration }, MaxAccessibility)
            .Where(x => IsAutoPropertyWithNoInitializer((PropertyDeclarationSyntax)x.Node)
                        && !HasStructLayoutAttribute(x.Symbol.ContainingType));
        return candidateFields.Concat(candidateProperties).ToList();
    }

    private static bool TypeDefinitionShouldBeAnalyzed(ITypeSymbol namedType) =>
        namedType.IsClassOrStruct()
        && !HasStructLayoutAttribute(namedType)
        && namedType.ContainingType is null
        && !namedType.HasAttribute(KnownType.System_SerializableAttribute);

    private static bool HasStructLayoutAttribute(ISymbol namedTypeSymbol) =>
        namedTypeSymbol.HasAttribute(KnownType.System_Runtime_InteropServices_StructLayoutAttribute);

    private static bool IsInitializedOrFixed(VariableDeclaratorSyntax declarator) =>
        declarator.Initializer is not null
        || (declarator.Parent.Parent is BaseFieldDeclarationSyntax fieldDeclaration
            && fieldDeclaration.Modifiers.Any(SyntaxKind.FixedKeyword));

    private static bool IsAutoPropertyWithNoInitializer(PropertyDeclarationSyntax declaration) =>
        declaration.Initializer is null
        && declaration.AccessorList is not null
        && declaration.AccessorList.Accessors.All(x => x.Body is null && x.ExpressionBody is null);

    private static IList<MemberUsage> MemberUsages(CSharpRemovableDeclarationCollector removableDeclarationCollector, HashSet<ISymbol> declaredPrivateSymbols)
    {
        var symbolNames = declaredPrivateSymbols.Select(x => x.Name).ToHashSet();
        var usages = removableDeclarationCollector.TypeDeclarations
            .SelectMany(x => x.Node.DescendantNodes().Select(SimpleName).WhereNotNull()
                                .Where(x => symbolNames.Contains(x.Identifier.ValueText))
                                .Select(node => new MemberUsage(node, x.Model.GetSymbolInfo(node).Symbol, x.Model)));

        return usages.Where(x => x.Symbol is IFieldSymbol or IPropertySymbol).ToList();

        static SimpleNameSyntax SimpleName(SyntaxNode node) =>
            node switch
            {
                IdentifierNameSyntax identifierName => identifierName,
                GenericNameSyntax genericName => genericName,
                _ => null
            };
    }

    private static ISet<ISymbol> AssignedMemberSymbols(IList<MemberUsage> memberUsages)
    {
        var assignedMembers = new HashSet<ISymbol>();

        foreach (var memberUsage in memberUsages)
        {
            var memberSymbol = memberUsage.Symbol;
            var node = RelevantNode(memberUsage.Node, memberSymbol);
            var parentNode = node.Parent;

            if (PreOrPostfixOpSyntaxKinds.Contains(parentNode.Kind())
                || (parentNode is AssignmentExpressionSyntax assignment && assignment.Left == node)
                || (parentNode is ArgumentSyntax argument && (!argument.RefOrOutKeyword.IsKind(SyntaxKind.None) || TupleExpressionSyntaxWrapper.IsInstance(argument.Parent)))
                || RefExpressionSyntaxWrapper.IsInstance(parentNode))
            {
                assignedMembers.Add(memberSymbol);
                assignedMembers.Add(memberSymbol.OriginalDefinition);
            }
        }

        return assignedMembers;
    }

    private static SyntaxNode RelevantNode(ExpressionSyntax node, ISymbol memberSymbol)
    {
        // Handle "expr.FieldName"
        if (node.Parent is MemberAccessExpressionSyntax simpleMemberAccess && simpleMemberAccess.Name == node)
        {
            node = simpleMemberAccess;
        }
        // Handle "expr?.FieldName"
        else if (node.Parent is MemberBindingExpressionSyntax memberBinding && memberBinding.Name == node)
        {
            node = memberBinding;
        }

        // Handle "((expr.FieldName))"
        node = node.GetSelfOrTopParenthesizedExpression();

        if (IsValueType(memberSymbol))
        {
            // Handle (((exp.FieldName)).Member1).Member2
            var parentMemberAccess = node.Parent as MemberAccessExpressionSyntax;
            while (IsParentMemberAccess(parentMemberAccess, node))
            {
                node = parentMemberAccess.GetSelfOrTopParenthesizedExpression();
                parentMemberAccess = node.Parent as MemberAccessExpressionSyntax;
            }

            node = node.GetSelfOrTopParenthesizedExpression();
        }
        return node;
    }

    private static bool IsParentMemberAccess(MemberAccessExpressionSyntax parent, ExpressionSyntax node) =>
        parent?.Expression == node;

    private static bool IsValueType(ISymbol symbol) =>
        symbol switch
        {
            IFieldSymbol field => field.Type.IsValueType,
            IPropertySymbol property => property.Type.IsValueType,
            _ => false
        };
}
