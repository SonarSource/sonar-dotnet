/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Core.Rules;

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
                    && c.Model.GetDeclaredSymbol(c.Node) is INamedTypeSymbol { } classSymbol
                    && !classSymbol.DerivesFrom(KnownType.System_Exception)
                    && !classSymbol.Implements(KnownType.System_Runtime_InteropServices_Exception))
                {
                    c.ReportIssue(Rule, classIdentifier);
                }
            },
            Language.SyntaxKind.ClassAndModuleDeclarations);
}
