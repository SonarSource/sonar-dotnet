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

namespace SonarAnalyzer.VisualBasic.Rules
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
                        if (c.Model.GetDeclaredSymbol(name) is IFieldSymbol symbol &&
                            IsCandidateSymbol(symbol) &&
                            !NamingPatterns.IsRegexMatch(symbol.Name, Pattern))
                        {
                            c.ReportIssue(SupportedDiagnostics[0], name, symbol.Name, Pattern);
                        }
                    }
                },
                SyntaxKind.FieldDeclaration);
        }
    }
}
