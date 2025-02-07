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
                if (IsExtensionMethod((TMethodDeclaration)c.Node, c.Model)
                    && c.Model.GetDeclaredSymbol(c.Node) is IMethodSymbol method
                    && method.IsExtensionMethod
                    && method.Parameters.Length > 0
                    && method.Parameters[0].Type.Is(KnownType.System_Object))
                {
                    c.ReportIssue(Rule, Language.Syntax.NodeIdentifier(c.Node).Value);
                }
            },
            Language.SyntaxKind.MethodDeclarations);
}
