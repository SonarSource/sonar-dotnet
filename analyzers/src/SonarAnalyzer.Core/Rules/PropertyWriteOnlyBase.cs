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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class PropertyWriteOnlyBase<TSyntaxKind, TPropertyDeclaration> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
        where TPropertyDeclaration : SyntaxNode
    {
        protected const string DiagnosticId = "S2376";

        protected abstract TSyntaxKind SyntaxKind { get; }

        protected abstract bool IsWriteOnlyProperty(TPropertyDeclaration prop);

        protected override string MessageFormat => "Provide a getter for '{0}' or replace the property with a 'Set{0}' method.";

        protected PropertyWriteOnlyBase() : base(DiagnosticId) { }

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    var prop = (TPropertyDeclaration)c.Node;
                    if (IsWriteOnlyProperty(prop) && Language.Syntax.NodeIdentifier(prop) is { }  identifier)
                    {
                        c.ReportIssue(SupportedDiagnostics[0], identifier, identifier.ValueText);
                    }
                },
                SyntaxKind);
    }
}
