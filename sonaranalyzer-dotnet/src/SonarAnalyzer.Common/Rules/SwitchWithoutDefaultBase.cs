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

using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.Common
{
    public abstract class SwitchWithoutDefaultBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S131";
        protected const string MessageFormat = "Add a '{0}' clause to this '{1}' statement.";

        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    }

    public abstract class SwitchWithoutDefaultBase<TLanguageKindEnum> : SwitchWithoutDefaultBase
        where TLanguageKindEnum : struct
    {
        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                GeneratedCodeRecognizer,
                c =>
                {
                    if (TryGetDiagnostic(c.Node, out var diagnostic))
                    {
                        c.ReportDiagnosticWhenActive(diagnostic);
                    }
                },
                SyntaxKindsOfInterest.ToArray());
        }

        protected abstract bool TryGetDiagnostic(SyntaxNode node, out Diagnostic diagnostic);

        public abstract ImmutableArray<TLanguageKindEnum> SyntaxKindsOfInterest { get; }
    }
}
