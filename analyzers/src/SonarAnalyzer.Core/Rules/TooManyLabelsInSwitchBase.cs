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

public abstract class TooManyLabelsInSwitchBase<TSyntaxKind, TSwitchStatementSyntax> : ParametrizedDiagnosticAnalyzer
    where TSyntaxKind : struct
    where TSwitchStatementSyntax : SyntaxNode
{
    protected const string MessageFormat = "Consider reworking this '{0}' to reduce the number of '{1}' clauses to at most {{0}} or have only one statement per '{1}'.";
    protected const string DiagnosticId = "S1479";
    private const int DefaultValueMaximum = 30;

    protected abstract DiagnosticDescriptor Rule { get; }

    protected abstract TSyntaxKind[] SyntaxKinds { get; }

    protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }

    protected abstract SyntaxNode GetExpression(TSwitchStatementSyntax statement);

    protected abstract int GetSectionsCount(TSwitchStatementSyntax statement);

    protected abstract bool AllSectionsAreOneLiners(TSwitchStatementSyntax statement);

    protected abstract Location GetKeywordLocation(TSwitchStatementSyntax statement);

    [RuleParameter("maximum", PropertyType.Integer, "Maximum number of case", DefaultValueMaximum)]
    public int Maximum { get; set; } = DefaultValueMaximum;

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
        ImmutableArray.Create(Rule);

    protected override void Initialize(SonarParametrizedAnalysisContext context) =>
        context.RegisterNodeAction(
            GeneratedCodeRecognizer,
            c =>
            {
                var switchNode = (TSwitchStatementSyntax)c.Node;

                if (c.Model.GetTypeInfo(GetExpression(switchNode)).Type is { TypeKind: not TypeKind.Enum }
                    && GetSectionsCount(switchNode) > Maximum
                    && !AllSectionsAreOneLiners(switchNode))
                {
                    c.ReportIssue(Rule, GetKeywordLocation(switchNode), Maximum.ToString());
                }
            },
            SyntaxKinds);
}
