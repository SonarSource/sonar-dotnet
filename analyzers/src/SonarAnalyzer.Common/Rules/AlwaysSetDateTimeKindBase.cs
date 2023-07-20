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
                && IsDateTimeConstructorWithoutKindParameter(c.Node, c.SemanticModel))
            {
                c.ReportIssue(CreateDiagnostic(Rule, c.Node.GetLocation()));
            }
        },
        ObjectCreationExpression);

    protected static bool IsDateTimeConstructorWithoutKindParameter(SyntaxNode objectCreation, SemanticModel semanticModel) =>
        semanticModel.GetSymbolInfo(objectCreation).Symbol is IMethodSymbol ctor
        && ctor.IsInType(KnownType.System_DateTime)
        && !ctor.Parameters.Any(x => x.IsType(KnownType.System_DateTimeKind));
}
