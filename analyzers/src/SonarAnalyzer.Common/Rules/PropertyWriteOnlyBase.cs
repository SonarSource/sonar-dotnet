/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class PropertyWriteOnlyBase<TSyntaxKind, TPropertyDeclaration> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TPropertyDeclaration : SyntaxNode
    {
        protected const string DiagnosticId = "S2376";
        private const string MessageFormat = "Provide a getter for '{0}' or replace the property with a 'Set{0}' method.";
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }
        protected abstract TSyntaxKind SyntaxKind { get; }

        protected abstract bool IsWriteOnlyProperty(TPropertyDeclaration prop);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected PropertyWriteOnlyBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    var prop = (TPropertyDeclaration)c.Node;

                    if (IsWriteOnlyProperty(prop)
                        && Language.Syntax.NodeIdentifier(prop) is { }  identifier)
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0], identifier.GetLocation(), identifier.ValueText));
                    }
                },
                SyntaxKind);
    }
}
