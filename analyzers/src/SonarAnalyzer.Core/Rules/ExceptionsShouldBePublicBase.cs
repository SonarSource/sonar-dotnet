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

namespace SonarAnalyzer.Rules;

public abstract class ExceptionsShouldBePublicBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S3871";

    private static readonly KnownType[] BaseTypes = new[]
    {
        KnownType.System_Exception,
        KnownType.System_ApplicationException,
        KnownType.System_SystemException
    };

    protected ExceptionsShouldBePublicBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (c.Model.GetDeclaredSymbol(c.Node) is INamedTypeSymbol classSymbol
                    && classSymbol.GetEffectiveAccessibility() != Accessibility.Public
                    && classSymbol.BaseType.IsAny(BaseTypes))
                {
                    c.ReportIssue(Rule, Language.Syntax.NodeIdentifier(c.Node).Value);
                }
            },
            Language.SyntaxKind.ClassAndRecordDeclarations);
}
