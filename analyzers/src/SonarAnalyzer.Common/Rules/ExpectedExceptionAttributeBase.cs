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

using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class ExpectedExceptionAttributeBase<TAttributeSyntax, TSyntaxKind> : SonarDiagnosticAnalyzer
        where TAttributeSyntax : SyntaxNode
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S907"; // TODO: update ID.
        internal const string MessageFormat = "Use an Assert method to test the thrown exception.";

        private readonly DiagnosticDescriptor rule;
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        protected ExpectedExceptionAttributeBase() =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, Language.RspecResources);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(Language.GeneratedCodeRecognizer, c =>
            {
                if (c.Node is TAttributeSyntax attribute
                    && NameOf(attribute).StartsWith("ExpectedException", StringComparison.OrdinalIgnoreCase))
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, attribute.GetLocation()));
                }
            },
            Language.SyntaxKind.Attribute);

        protected abstract string NameOf(TAttributeSyntax attribute);
    }
}
