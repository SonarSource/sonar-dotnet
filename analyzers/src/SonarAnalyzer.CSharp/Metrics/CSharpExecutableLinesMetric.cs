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

namespace SonarAnalyzer.CSharp.Metrics;

public static class CSharpExecutableLinesMetric
{
    public static ImmutableArray<int> GetLineNumbers(SyntaxTree syntaxTree, SemanticModel semanticModel)
    {
        var walker = GetWalker(syntaxTree, semanticModel);
        walker.SafeVisit(syntaxTree.GetRoot());
        return walker.ExecutableLineNumbers.ToImmutableArray();
    }

    private static ExecutableLinesWalker GetWalker(SyntaxTree syntaxTree, SemanticModel semanticModel) =>
        GeneratedCodeRecognizer.IsRazor(syntaxTree)
            ? new RazorExecutableLinesWalker(semanticModel)
            : new ExecutableLinesWalker(semanticModel);

    private class ExecutableLinesWalker : SafeCSharpSyntaxWalker
    {
        private readonly SemanticModel model;

        public HashSet<int> ExecutableLineNumbers { get; } = new();

        protected virtual bool AddExecutableLineNumbers(Location location)
        {
            ExecutableLineNumbers.Add(location.GetLineNumberToReport());
            return true;
        }

        public ExecutableLinesWalker(SemanticModel model) =>
            this.model = model;

        public override void DefaultVisit(SyntaxNode node)
        {
            if (FindExecutableLines(node))
            {
                base.DefaultVisit(node);
            }
        }

        private bool FindExecutableLines(SyntaxNode node)
        {
            switch (node.Kind())
            {
                case SyntaxKind.AttributeList:
                    return false;

                case SyntaxKind.CheckedStatement:
                case SyntaxKind.UncheckedStatement:

                case SyntaxKind.LockStatement:
                case SyntaxKind.FixedStatement:
                case SyntaxKind.UnsafeStatement:
                case SyntaxKind.UsingStatement:

                case SyntaxKind.EmptyStatement:
                case SyntaxKind.ExpressionStatement:

                case SyntaxKind.DoStatement:
                case SyntaxKind.ForEachStatement:
                case SyntaxKind.ForStatement:
                case SyntaxKind.WhileStatement:

                case SyntaxKind.IfStatement:
                case SyntaxKind.LabeledStatement:
                case SyntaxKind.SwitchStatement:
                case SyntaxKind.ConditionalAccessExpression:
                case SyntaxKind.ConditionalExpression:

                case SyntaxKind.GotoStatement:
                case SyntaxKind.ThrowStatement:
                case SyntaxKind.ReturnStatement:
                case SyntaxKind.BreakStatement:
                case SyntaxKind.ContinueStatement:

                case SyntaxKind.YieldBreakStatement:
                case SyntaxKind.YieldReturnStatement:

                case SyntaxKind.SimpleMemberAccessExpression:
                case SyntaxKind.InvocationExpression:

                case SyntaxKind.SimpleLambdaExpression:
                case SyntaxKind.ParenthesizedLambdaExpression:

                case SyntaxKind.ArrayInitializerExpression:
                    return AddExecutableLineNumbers(node.GetLocation());

                case SyntaxKind.StructDeclaration:
                case SyntaxKind.ClassDeclaration:
                case SyntaxKindEx.RecordDeclaration:
                case SyntaxKindEx.RecordStructDeclaration:
                    return !HasExcludedCodeAttribute(node, ((BaseTypeDeclarationSyntax)node).AttributeLists, true);

                case SyntaxKind.MethodDeclaration:
                case SyntaxKind.ConstructorDeclaration:
                    return !HasExcludedCodeAttribute(node, ((BaseMethodDeclarationSyntax)node).AttributeLists, true);

                case SyntaxKind.PropertyDeclaration:
                case SyntaxKind.EventDeclaration:
                    return !HasExcludedCodeAttribute(node, ((BasePropertyDeclarationSyntax)node).AttributeLists, false);

                case SyntaxKind.AddAccessorDeclaration:
                case SyntaxKind.RemoveAccessorDeclaration:
                case SyntaxKind.SetAccessorDeclaration:
                case SyntaxKind.GetAccessorDeclaration:
                case SyntaxKindEx.InitAccessorDeclaration:
                    return !HasExcludedCodeAttribute(node, ((AccessorDeclarationSyntax)node).AttributeLists, false);

                default:
                    return true;
            }
        }

        private bool HasExcludedCodeAttribute(SyntaxNode node, SyntaxList<AttributeListSyntax> attributeLists, bool canBePartial)
        {
            var hasExcludeFromCodeCoverageAttribute = attributeLists.SelectMany(x => x.Attributes).Any(IsExcludedAttribute);
            return hasExcludeFromCodeCoverageAttribute || !canBePartial
                ? hasExcludeFromCodeCoverageAttribute
                : model.GetDeclaredSymbol(node) is { Kind: SymbolKind.Method or SymbolKind.NamedType} symbol
                  && symbol.HasAttribute(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute);
        }

        private bool IsExcludedAttribute(AttributeSyntax attribute) =>
            attribute.IsKnownType(KnownType.System_Diagnostics_CodeAnalysis_ExcludeFromCodeCoverageAttribute, model);
    }

    private sealed class RazorExecutableLinesWalker : ExecutableLinesWalker
    {
        public RazorExecutableLinesWalker(SemanticModel model) : base(model) { }

        protected override bool AddExecutableLineNumbers(Location location)
        {
            var mappedLocation = location.GetMappedLineSpan();
            if (mappedLocation.HasMappedPath)
            {
                ExecutableLineNumbers.Add(mappedLocation.GetLineNumberToReport());
            }
            return true;
        }
    }
}
