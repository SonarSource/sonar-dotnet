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

    private static readonly DiagnosticDescriptor Rule =
        DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ISet<SyntaxKind> PreOrPostfixOpSyntaxKinds = new HashSet<SyntaxKind>
    {
        SyntaxKind.PostDecrementExpression,
        SyntaxKind.PostIncrementExpression,
        SyntaxKind.PreDecrementExpression,
        SyntaxKind.PreIncrementExpression,
    };

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterSymbolAction(
            c =>
            {
                var namedType = (INamedTypeSymbol)c.Symbol;
                if (TypeDefinitionShouldBeSkipped(namedType))
                {
                    return;
                }

                var removableDeclarationCollector = new CSharpRemovableDeclarationCollector(namedType, c.Compilation);

                var allCandidateMembers = GetCandidateDeclarations(removableDeclarationCollector);
                if (!allCandidateMembers.Any())
                {
                    return;
                }

                var usedMembers = GetMemberUsages(removableDeclarationCollector, new HashSet<ISymbol>(allCandidateMembers.Select(t => t.Symbol)));
                var usedMemberSymbols = new HashSet<ISymbol>(usedMembers.Select(tuple => tuple.Symbol));

                var assignedMemberSymbols = GetAssignedMemberSymbols(usedMembers);
                var unassignedUsedMemberSymbols = allCandidateMembers.Where(x => usedMemberSymbols.Contains(x.Symbol) && !assignedMemberSymbols.Contains(x.Symbol));

                foreach (var candidateMember in unassignedUsedMemberSymbols)
                {
                    var field = candidateMember.Node as VariableDeclaratorSyntax;
                    var property = candidateMember.Node as PropertyDeclarationSyntax;

                    var memberType = field is null ? "auto-property" : "field";

                    var location = field is null
                        ? property.Identifier.GetLocation()
                        : field.Identifier.GetLocation();

                    c.ReportIssue(Rule, location, memberType, candidateMember.Symbol.Name);
                }
            },
            SymbolKind.NamedType);

    private static List<NodeSymbolAndModel<SyntaxNode, ISymbol>> GetCandidateDeclarations(CSharpRemovableDeclarationCollector removableDeclarationCollector)
    {
        var candidateFields = removableDeclarationCollector.RemovableFieldLikeDeclarations(new HashSet<SyntaxKind> { SyntaxKind.FieldDeclaration }, MaxAccessibility)
                                                           .Where(x => !IsInitializedOrFixed((VariableDeclaratorSyntax)x.Node)
                                                               && !HasStructLayoutAttribute(x.Symbol.ContainingType));

        var candidateProperties = removableDeclarationCollector.RemovableDeclarations(new HashSet<SyntaxKind> { SyntaxKind.PropertyDeclaration }, MaxAccessibility)
                                                               .Where(x => IsAutoPropertyWithNoInitializer((PropertyDeclarationSyntax)x.Node)
                                                                   && !HasStructLayoutAttribute(x.Symbol.ContainingType));

        return candidateFields.Concat(candidateProperties).ToList();
    }

    private static bool TypeDefinitionShouldBeSkipped(ITypeSymbol namedType) =>
        !namedType.IsClassOrStruct()
        || HasStructLayoutAttribute(namedType)
        || namedType.ContainingType is not null
        || namedType.HasAttribute(KnownType.System_SerializableAttribute);

    private static bool HasStructLayoutAttribute(ISymbol namedTypeSymbol) =>
        namedTypeSymbol.HasAttribute(KnownType.System_Runtime_InteropServices_StructLayoutAttribute);

    private static bool IsInitializedOrFixed(VariableDeclaratorSyntax declarator)
    {
        if (declarator.Parent.Parent is BaseFieldDeclarationSyntax fieldDeclaration
            && fieldDeclaration.Modifiers.Any(SyntaxKind.FixedKeyword))
        {
            return true;
        }

        return declarator.Initializer is not null;
    }

    private static bool IsAutoPropertyWithNoInitializer(PropertyDeclarationSyntax declaration) =>
        declaration.Initializer is null
        && declaration.AccessorList is not null
        && declaration.AccessorList.Accessors.All(acc => acc.Body is null && acc.ExpressionBody is null);

    private static IList<MemberUsage> GetMemberUsages(CSharpRemovableDeclarationCollector removableDeclarationCollector, HashSet<ISymbol> declaredPrivateSymbols)
    {
        var symbolNames = declaredPrivateSymbols.Select(s => s.Name).ToHashSet();

        var identifiers = removableDeclarationCollector.TypeDeclarations
            .SelectMany(container => container.Node.DescendantNodes()
                .Where(node => node.IsKind(SyntaxKind.IdentifierName))
                .Cast<IdentifierNameSyntax>()
                .Where(x => symbolNames.Contains(x.Identifier.ValueText))
                .Select(x => new MemberUsage(x, container.Model.GetSymbolInfo(x).Symbol, container.Model)));

        var generic = removableDeclarationCollector.TypeDeclarations
            .SelectMany(container => container.Node.DescendantNodes()
                .Where(node => node.IsKind(SyntaxKind.GenericName))
                .Cast<GenericNameSyntax>()
                .Where(x => symbolNames.Contains(x.Identifier.ValueText))
                .Select(x => new MemberUsage(x, container.Model.GetSymbolInfo(x).Symbol, container.Model)));

        return identifiers.Concat(generic)
            .Where(x => x.Symbol is IFieldSymbol or IPropertySymbol)
            .ToList();
    }

    private static ISet<ISymbol> GetAssignedMemberSymbols(IList<MemberUsage> memberUsages)
    {
        var assignedMembers = new HashSet<ISymbol>();

        foreach (var memberUsage in memberUsages)
        {
            ExpressionSyntax node = memberUsage.Node;
            var memberSymbol = memberUsage.Symbol;

            // Handle "expr.FieldName"
            if (node.Parent is MemberAccessExpressionSyntax simpleMemberAccess
                && simpleMemberAccess.Name == node)
            {
                node = simpleMemberAccess;
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
