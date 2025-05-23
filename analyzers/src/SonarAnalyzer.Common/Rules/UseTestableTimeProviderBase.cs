﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
                && c.SemanticModel.GetSymbolInfo(c.Node).Symbol is IPropertySymbol property
                && property.IsInType(trackedTypes)
                && !c.Node.Ancestors().Any(x => Ignore(x, c.SemanticModel)))
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
