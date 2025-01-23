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

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    public sealed class PropertyWithArrayType : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2365";
        private const string MessageFormat = "Refactor '{0}' into a method, properties should not be based on arrays.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    var property = (PropertyStatementSyntax)c.Node;
                    if (property.ImplementsClause is null
                        && !property.Modifiers.Any(x => x.IsKind(SyntaxKind.OverridesKeyword))
                        && c.Model.GetDeclaredSymbol(property) is IPropertySymbol symbol
                        && !symbol.IsAutoProperty()
                        && symbol.Type is IArrayTypeSymbol)
                    {
                        c.ReportIssue(Rule, property.Identifier, symbol.Name);
                    }
                },
                SyntaxKind.PropertyStatement);
    }
}
