/*
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

namespace SonarAnalyzer.Rules.VisualBasic
{
    public abstract class FieldNameChecker : ParametrizedDiagnosticAnalyzer
    {
        public virtual string Pattern { get; set; }

        protected abstract bool IsCandidateSymbol(IFieldSymbol symbol);

        protected sealed override void Initialize(SonarParametrizedAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                    foreach (var name in fieldDeclaration.Declarators.SelectMany(v => v.Names).WhereNotNull())
                    {
                        if (c.SemanticModel.GetDeclaredSymbol(name) is IFieldSymbol symbol &&
                            IsCandidateSymbol(symbol) &&
                            !NamingHelper.IsRegexMatch(symbol.Name, Pattern))
                        {
                            c.ReportIssue(SupportedDiagnostics[0], name, symbol.Name, Pattern);
                        }
                    }
                },
                SyntaxKind.FieldDeclaration);
        }
    }
}
