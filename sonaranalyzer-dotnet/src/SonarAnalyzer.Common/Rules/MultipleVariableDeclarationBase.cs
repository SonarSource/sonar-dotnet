/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class MultipleVariableDeclarationBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1659";
        protected const string MessageFormat = "Declare '{0}' in a separate statement.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class MultipleVariableDeclarationBase<TLanguageKindEnum,
        TFieldDeclarationSyntax, TLocalDeclarationSyntax> : MultipleVariableDeclarationBase
        where TLanguageKindEnum : struct
        where TFieldDeclarationSyntax: SyntaxNode
        where TLocalDeclarationSyntax: SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var local = (TLocalDeclarationSyntax)c.Node;
                    CheckAndReportVariables(GetIdentifiers(local).ToList(), c, SupportedDiagnostics[0]);
                },
                LocalDeclarationKind);

            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var field = (TFieldDeclarationSyntax)c.Node;
                    CheckAndReportVariables(GetIdentifiers(field).ToList(), c, SupportedDiagnostics[0]);
                },
                FieldDeclarationKind);
        }

        private static void CheckAndReportVariables(IList<SyntaxToken> variables, SyntaxNodeAnalysisContext context, DiagnosticDescriptor rule)
        {
            if (variables.Count <= 1)
            {
                return;
            }
            foreach (var variable in variables.Skip(1))
            {
                context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, variable.GetLocation(), variable.ValueText));
            }
        }

        protected abstract IEnumerable<SyntaxToken> GetIdentifiers(TLocalDeclarationSyntax node);

        protected abstract IEnumerable<SyntaxToken> GetIdentifiers(TFieldDeclarationSyntax node);

        public abstract TLanguageKindEnum LocalDeclarationKind { get; }
        public abstract TLanguageKindEnum FieldDeclarationKind { get; }
    }
}
