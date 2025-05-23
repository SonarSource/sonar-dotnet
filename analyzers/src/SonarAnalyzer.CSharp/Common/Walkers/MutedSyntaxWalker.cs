﻿/*
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

namespace SonarAnalyzer.Common.Walkers;

/// <summary>
/// This class find syntax cases that are not properly supported by current CFG/SE/LVA and we mute all issues related to these scenarios.
/// </summary>
internal class MutedSyntaxWalker : CSharpSyntaxWalker
{
    // All kinds that SonarAnalysisContextExtensions.RegisterExplodedGraphBasedAnalysis registers for
    private static readonly SyntaxKind[] RootKinds =
    [
        SyntaxKind.AddAccessorDeclaration,
        SyntaxKind.AnonymousMethodExpression,
        SyntaxKind.ConstructorDeclaration,
        SyntaxKind.ConversionOperatorDeclaration,
        SyntaxKind.DestructorDeclaration,
        SyntaxKind.GetAccessorDeclaration,
        SyntaxKindEx.InitAccessorDeclaration,
        SyntaxKind.MethodDeclaration,
        SyntaxKind.OperatorDeclaration,
        SyntaxKind.ParenthesizedLambdaExpression,
        SyntaxKind.PropertyDeclaration,
        SyntaxKind.RemoveAccessorDeclaration,
        SyntaxKind.SetAccessorDeclaration,
        SyntaxKind.SimpleLambdaExpression
    ];

    private readonly SemanticModel model;
    private readonly SyntaxNode root;
    private readonly ISymbol[] symbols;
    private bool isMuted;

    public MutedSyntaxWalker(SemanticModel model, SyntaxNode node)
        : this(model, node, node.DescendantNodesAndSelf().OfType<IdentifierNameSyntax>().Select(x => model.GetSymbolInfo(x).Symbol).WhereNotNull().ToArray()) { }

    public MutedSyntaxWalker(SemanticModel model, SyntaxNode node, params ISymbol[] symbols)
    {
        this.model = model;
        this.symbols = symbols;
        root = node.Ancestors().FirstOrDefault(x => x.IsAnyKind(RootKinds));
    }

    public bool IsMuted()
    {
        if (symbols.Any() && root is not null)
        {
            Visit(root);
        }
        return isMuted;
    }

    public override void Visit(SyntaxNode node)
    {
        if (!isMuted)   // Performance optimization, we can stop visiting once we know the answer
        {
            base.Visit(node);
        }
    }

    public override void VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (Array.Find(symbols, x => node.NameIs(x.Name) && x.Equals(model.GetSymbolInfo(node).Symbol)) is { } symbol)
        {
            isMuted = IsInTupleAssignmentTarget() || IsUsedInLocalFunction(symbol) || IsInUnsupportedExpression();
        }
        base.VisitIdentifierName(node);

        bool IsInTupleAssignmentTarget() =>
            node.Parent is ArgumentSyntax argument && argument.IsInTupleAssignmentTarget();

        bool IsUsedInLocalFunction(ISymbol symbol) =>
            // We don't mute it if it's declared and used in local function
            !(symbol.ContainingSymbol is IMethodSymbol containingSymbol && containingSymbol.MethodKind == MethodKindEx.LocalFunction)
            && node.HasAncestor(SyntaxKindEx.LocalFunctionStatement);

        bool IsInUnsupportedExpression() =>
            node.FirstAncestorOrSelf<SyntaxNode>(x => x.IsAnyKind(SyntaxKindEx.IndexExpression, SyntaxKindEx.RangeExpression)) is not null;
    }
}
