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

public abstract class ValueTypeShouldImplementIEquatableBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S3898";

    protected override string MessageFormat => "Implement 'IEquatable<T>' in value type '{0}'.";

    protected ValueTypeShouldImplementIEquatableBase() : base(DiagnosticId) { }

    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                var modifiers = Language.Syntax.ModifierKinds(c.Node);
                if (!modifiers.Any(x => x.Equals(Language.SyntaxKind.RefKeyword))
                    && c.SemanticModel.GetDeclaredSymbol(c.Node) is INamedTypeSymbol structSymbol
                    && !structSymbol.Implements(KnownType.System_IEquatable_T))
                {
                    var identifier = Language.Syntax.NodeIdentifier(c.Node).Value;
                    c.ReportIssue(CreateDiagnostic(Rule, identifier.GetLocation(), identifier.ValueText));
                }
            },
            Language.SyntaxKind.StructDeclaration);
}
