/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class ClassShouldNotBeEmptyBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2094";

    private static readonly ImmutableArray<KnownType> SubClassesToIgnore = ImmutableArray.Create(
        KnownType.Microsoft_AspNetCore_Mvc_RazorPages_PageModel,
        KnownType.System_Exception);

    protected override string MessageFormat => "Remove this empty {0}, or add members to it.";

    protected abstract bool IsEmpty(SyntaxNode node);
    protected abstract bool IsClassWithDeclaredBaseClass(SyntaxNode node);
    protected abstract string DeclarationTypeKeyword(SyntaxNode node);

    protected ClassShouldNotBeEmptyBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (Language.Syntax.NodeIdentifier(c.Node) is { IsMissing: false } identifier
                    && IsEmpty(c.Node)
                    && !ShouldIgnoreBecauseOfBaseClass(c.Node, c.SemanticModel))
                {
                    c.ReportIssue(Diagnostic.Create(Rule, identifier.GetLocation(), DeclarationTypeKeyword(c.Node)));
                }

                bool ShouldIgnoreBecauseOfBaseClass(SyntaxNode node, SemanticModel model) =>
                    IsClassWithDeclaredBaseClass(node)
                    && model.GetDeclaredSymbol(node) is INamedTypeSymbol classSymbol
                    && classSymbol.DerivesFromAny(SubClassesToIgnore);
            },
            Language.SyntaxKind.ClassAndRecordClassDeclarations);
}
