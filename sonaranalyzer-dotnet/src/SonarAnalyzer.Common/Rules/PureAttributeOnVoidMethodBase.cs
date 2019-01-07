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

namespace SonarAnalyzer.Rules
{
    public abstract class PureAttributeOnVoidMethodBase : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S3603";
        protected const string MessageFormat = "Remove the 'Pure' attribute or change the method to return a value.";

        protected abstract DiagnosticDescriptor Rule { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var methodSymbol = (IMethodSymbol)c.Symbol;
                    if (methodSymbol == null ||
                        !methodSymbol.ReturnsVoid ||
                        methodSymbol.Parameters.Any(p => p.RefKind != RefKind.None))
                    {
                        return;
                    }

                    var pureAttribute = methodSymbol
                        .GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass.Is(KnownType.System_Diagnostics_Contracts_PureAttribute));

                    if (pureAttribute != null)
                    {
                        c.ReportDiagnosticWhenActive(
                            Diagnostic.Create(Rule, pureAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation()));
                    }
                }, SymbolKind.Method);
    }
}
