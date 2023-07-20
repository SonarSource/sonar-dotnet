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

            context.ReportIssue(CreateDiagnostic(rule, controlVar.GetLocation(), Pattern));
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

                context.ReportIssue(CreateDiagnostic(rule, name.Identifier.GetLocation(), Pattern));
            }
        }
    }
}
