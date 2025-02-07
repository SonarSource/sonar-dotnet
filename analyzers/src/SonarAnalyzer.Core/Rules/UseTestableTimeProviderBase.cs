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

public abstract class UseTestableTimeProviderBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
     where TSyntaxKind : struct
{
    private const string DiagnosticId = "S6354";

    protected override string MessageFormat => "Use a testable (date) time provider instead.";

    private readonly ImmutableArray<KnownType> trackedTypes = ImmutableArray.Create(KnownType.System_DateTime, KnownType.System_DateTimeOffset);

    protected abstract bool Ignore(SyntaxNode ancestor, SemanticModel semanticModel);

    protected UseTestableTimeProviderBase() : base(DiagnosticId) { }
    protected sealed override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer, c =>
        {
            if (IsDateTimeProviderProperty(Language.Syntax.NodeIdentifier(c.Node).Value.Text)
                && c.Model.GetSymbolInfo(c.Node).Symbol is IPropertySymbol property
                && property.IsInType(trackedTypes)
                && !c.Node.Ancestors().Any(x => Ignore(x, c.Model)))
            {
                c.ReportIssue(Rule, c.Node.Parent);
            }
        },
        Language.SyntaxKind.IdentifierName);

    private bool IsDateTimeProviderProperty(string name) =>
        nameof(DateTime.Now).Equals(name, Language.NameComparison)
        || nameof(DateTime.UtcNow).Equals(name, Language.NameComparison)
        || nameof(DateTime.Today).Equals(name, Language.NameComparison);
}
