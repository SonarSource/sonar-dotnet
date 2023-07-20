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

public abstract class ExtensionMethodShouldNotExtendObjectBase<TSyntaxKind, TMethodDeclaration> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TMethodDeclaration : SyntaxNode
{
    private const string DiagnosticId = "S4225";

    protected override string MessageFormat => "Refactor this extension to extend a more concrete type.";

    protected abstract bool IsExtensionMethod(TMethodDeclaration declaration, SemanticModel semanticModel);

    protected ExtensionMethodShouldNotExtendObjectBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (IsExtensionMethod((TMethodDeclaration)c.Node, c.SemanticModel)
                    && c.SemanticModel.GetDeclaredSymbol(c.Node) is IMethodSymbol method
                    && method.IsExtensionMethod
                    && method.Parameters.Length > 0
                    && method.Parameters[0].Type.Is(KnownType.System_Object))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, Language.Syntax.NodeIdentifier(c.Node).Value.GetLocation()));
                }
            },
            Language.SyntaxKind.MethodDeclarations);
}
