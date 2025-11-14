/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules;

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
                    && c.Model.GetDeclaredSymbol(c.Node) is INamedTypeSymbol structSymbol
                    && !structSymbol.Implements(KnownType.System_IEquatable_T))
                {
                    var identifier = Language.Syntax.NodeIdentifier(c.Node).Value;
                    c.ReportIssue(Rule, identifier, identifier.ValueText);
                }
            },
            Language.SyntaxKind.StructDeclaration);
}
