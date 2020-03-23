/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class GotoStatementBase : SonarDiagnosticAnalyzer
    {
        protected const string DiagnosticId = "S907";
        internal const string MessageFormat = "Remove this use of 'goto'.";

        public sealed override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);
        protected abstract GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
        protected abstract DiagnosticDescriptor Rule { get; }
    }

    public abstract class GotoStatementBase<TLanguageKindEnum> : GotoStatementBase
         where TLanguageKindEnum : struct
    {
        protected abstract ImmutableArray<TLanguageKindEnum> GotoSyntaxKinds { get; }

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
               GeneratedCodeRecognizer,
                c =>
                {
                    c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, c.Node.GetFirstToken().GetLocation()));
                },
                GotoSyntaxKinds.ToArray());
        }
    }
}
