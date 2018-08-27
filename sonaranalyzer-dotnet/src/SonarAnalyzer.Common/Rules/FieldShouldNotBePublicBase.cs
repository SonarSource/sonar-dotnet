/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class FieldShouldNotBePublicBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S2357";
        protected const string MessageFormat = "Make '{0}' private.";

        protected static bool FieldIsRelevant(IFieldSymbol fieldSymbol)
        {
            return fieldSymbol != null &&
                   !fieldSymbol.IsStatic &&
                   !fieldSymbol.IsConst &&
                   fieldSymbol.GetEffectiveAccessibility() == Accessibility.Public &&
                   fieldSymbol.ContainingType.IsClass();
        }

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class FieldShouldNotBePublicBase<TLanguageKindEnum, TFieldDeclarationSyntax, TVariableSyntax> : FieldShouldNotBePublicBase
        where TLanguageKindEnum : struct
        where TFieldDeclarationSyntax : SyntaxNode
        where TVariableSyntax : SyntaxNode
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    var fieldDeclaration = (TFieldDeclarationSyntax)c.Node;
                    var variables = GetVariables(fieldDeclaration);

                    foreach (var variable in variables
                        .Select(variableDeclaratorSyntax => new
                        {
                            Syntax = variableDeclaratorSyntax,
                            Symbol = c.SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol
                        })
                        .Where(f => FieldIsRelevant(f.Symbol)))
                    {
                        var identifier = GetIdentifier(variable.Syntax);
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], identifier.GetLocation(),
                            identifier.ValueText));
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }

        protected abstract IEnumerable<TVariableSyntax> GetVariables(TFieldDeclarationSyntax fieldDeclaration);

        protected abstract SyntaxToken GetIdentifier(TVariableSyntax variable);
    }
}
