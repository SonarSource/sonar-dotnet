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

namespace SonarAnalyzer.Rules
{
    public abstract class FieldShouldNotBePublicBase<TSyntaxKind, TFieldDeclarationSyntax, TVariableSyntax> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TFieldDeclarationSyntax : SyntaxNode
        where TVariableSyntax : SyntaxNode
    {
        private const string DiagnosticId = "S2357";

        protected abstract IEnumerable<TVariableSyntax> Variables(TFieldDeclarationSyntax fieldDeclaration);

        protected override string MessageFormat => "Make '{0}' private.";

        protected FieldShouldNotBePublicBase() : base(DiagnosticId) { }

        protected static bool FieldIsRelevant(IFieldSymbol fieldSymbol) =>
            fieldSymbol is { IsStatic: false, IsConst: false }
            && fieldSymbol.GetEffectiveAccessibility() == Accessibility.Public
            && fieldSymbol.ContainingType.IsClass();

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    var fieldDeclaration = (TFieldDeclarationSyntax)c.Node;
                    var variables = Variables(fieldDeclaration);

                    foreach (var variable in variables.Select(x => new Pair(x, c.SemanticModel.GetDeclaredSymbol(x) as IFieldSymbol)).Where(x => FieldIsRelevant(x.Symbol)))
                    {
                        var identifier = Language.Syntax.NodeIdentifier(variable.Node);
                        c.ReportIssue(CreateDiagnostic(Rule, identifier.Value.GetLocation(), identifier.Value.ValueText));
                    }
                },
                Language.SyntaxKind.FieldDeclaration);

        private sealed record Pair(TVariableSyntax Node, IFieldSymbol Symbol);
    }
}
