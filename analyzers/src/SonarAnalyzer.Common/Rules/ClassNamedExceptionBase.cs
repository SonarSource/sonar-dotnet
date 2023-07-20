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

public abstract class ClassNamedExceptionBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S2166";

    protected override string MessageFormat => "Rename this class to remove \"Exception\" or correct its inheritance.";

    protected ClassNamedExceptionBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (Language.Syntax.NodeIdentifier(c.Node) is { IsMissing: false } classIdentifier
                    && classIdentifier.ValueText.EndsWith("Exception", StringComparison.InvariantCultureIgnoreCase)
                    && c.SemanticModel.GetDeclaredSymbol(c.Node) is INamedTypeSymbol { } classSymbol
                    && !classSymbol.DerivesFrom(KnownType.System_Exception)
                    && !classSymbol.Implements(KnownType.System_Runtime_InteropServices_Exception))
                {
                    c.ReportIssue(CreateDiagnostic(Rule, classIdentifier.GetLocation()));
                }
            },
            Language.SyntaxKind.ClassAndModuleDeclarations);
}
