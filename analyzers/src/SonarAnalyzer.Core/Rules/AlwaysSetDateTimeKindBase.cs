/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

public abstract class AlwaysSetDateTimeKindBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6562";

    protected abstract TSyntaxKind ObjectCreationExpression { get; }
    protected abstract string[] ValidNames { get; }

    protected override string MessageFormat => "Provide the \"DateTimeKind\" when creating this object.";

    protected AlwaysSetDateTimeKindBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (Language.Syntax.ObjectCreationTypeIdentifier(c.Node) is { IsMissing: false } identifier
                && Array.Exists(ValidNames, x => x.Equals(identifier.ValueText, Language.NameComparison))
                && IsDateTimeConstructorWithoutKindParameter(c.Node, c.Model))
            {
                c.ReportIssue(Rule, c.Node);
            }
        },
        ObjectCreationExpression);

    protected static bool IsDateTimeConstructorWithoutKindParameter(SyntaxNode objectCreation, SemanticModel semanticModel) =>
        semanticModel.GetSymbolInfo(objectCreation).Symbol is IMethodSymbol ctor
        && ctor.IsInType(KnownType.System_DateTime)
        && !ctor.Parameters.Any(x => x.IsType(KnownType.System_DateTimeKind));
}
