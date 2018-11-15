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

using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public delegate bool ClassDeclarationCondition(INamedTypeSymbol classSymbol, out Location issueLocation);

    public abstract class ClassDeclarationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected ClassDeclarationTracker(IAnalyzerConfiguration analysisConfiguration, DiagnosticDescriptor rule)
            : base(analysisConfiguration, rule)
        {
        }

        public void Track(SonarAnalysisContext context, params ClassDeclarationCondition[] conditions)
        {
            context.RegisterCompilationStartAction(
                c =>
                {
                    if (IsEnabled(c.Options))
                    {
                        c.RegisterSymbolAction(
                            TrackClassDefinition,
                            SymbolKind.NamedType);
                    }
                });


            void TrackClassDefinition(SymbolAnalysisContext c)
            {
                if (IsTrackedClass(c.Symbol as INamedTypeSymbol, out var issueLocation))
                {
                    c.ReportDiagnosticIfNonGenerated(GeneratedCodeRecognizer, Diagnostic.Create(Rule, issueLocation));
                }
            }

            bool IsTrackedClass(INamedTypeSymbol symbol, out Location issueLocation)
            {
                // We can't pass the issueLocation to the lambda directly so we need a temporary variable
                Location locationToReport = null;
                if (conditions.All(c => c(symbol, out locationToReport)))
                {
                    issueLocation = locationToReport;
                    return true;
                }

                issueLocation = Location.None;
                return false;
            }
        }

        #region Standard checks
        // Add re-usable checks here...

        #endregion
    }
}
