/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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
    public sealed class LocalVariableName : ParametrizedDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S117";
        private const string MessageFormat = "Rename this local variable to match the regular expression: '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat,
                isEnabledByDefault: false);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        [RuleParameter("format", PropertyType.String,
            "Regular expression used to check the local variable names against.", NamingHelper.CamelCasingPattern)]
        public string Pattern { get; set; } = NamingHelper.CamelCasingPattern;

        protected override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                ProcessVariableDeclarator,
                SyntaxKind.VariableDeclarator);

            context.RegisterNodeAction(
                c => ProcessLoop(c, (ForStatementSyntax)c.Node, f => f.ControlVariable, s => s.IsFor()),
                SyntaxKind.ForStatement);

            context.RegisterNodeAction(
                c => ProcessLoop(c, (ForEachStatementSyntax)c.Node, f => f.ControlVariable, s => s.IsForEach()),
                SyntaxKind.ForEachStatement);
        }

        private void ProcessLoop<T>(SonarSyntaxNodeReportingContext context, T loop, Func<T, VisualBasicSyntaxNode> GetControlVariable, Func<ILocalSymbol, bool> isDeclaredInLoop)
        {
            var controlVar = GetControlVariable(loop);
            if (!(controlVar is IdentifierNameSyntax))
            {
                return;
            }

            if (!(context.SemanticModel.GetSymbolInfo(controlVar).Symbol is ILocalSymbol symbol) ||
                !isDeclaredInLoop(symbol) ||
                NamingHelper.IsRegexMatch(symbol.Name, Pattern))
            {
                return;
            }

            context.ReportIssue(rule, controlVar, Pattern);
        }

        private void ProcessVariableDeclarator(SonarSyntaxNodeReportingContext context)
        {
            var declarator = (VariableDeclaratorSyntax)context.Node;
            if (declarator.Parent is FieldDeclarationSyntax)
            {
                return;
            }

            foreach (var name in declarator.Names
                .Where(n => n != null &&
                    !NamingHelper.IsRegexMatch(n.Identifier.ValueText, Pattern)))
            {
                if (!(context.SemanticModel.GetDeclaredSymbol(name) is ILocalSymbol symbol) ||
                    symbol.IsConst)
                {
                    continue;
                }

                context.ReportIssue(rule, name.Identifier, Pattern);
            }
        }
    }
}
