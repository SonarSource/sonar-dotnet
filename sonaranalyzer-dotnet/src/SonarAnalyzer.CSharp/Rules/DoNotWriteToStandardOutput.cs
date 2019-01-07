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
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    // This base class is only there to avoid duplication between the implementation of S106 and S2228
    public abstract class DoNotWriteToStandardOutputBase : SonarDiagnosticAnalyzer
    {
        private static readonly ISet<string> BannedConsoleMembers = new HashSet<string> { "WriteLine", "Write" };

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    if (c.Compilation.Options.OutputKind == OutputKind.ConsoleApplication)
                    {
                        return;
                    }

                    var methodCall = (InvocationExpressionSyntax)c.Node;
                    var methodSymbol = c.SemanticModel.GetSymbolInfo(methodCall.Expression).Symbol;

                    if (methodSymbol != null &&
                        methodSymbol.IsInType(KnownType.System_Console) &&
                        BannedConsoleMembers.Contains(methodSymbol.Name) &&
                        !CSharpDebugOnlyCodeHelper.IsInDebugBlock(c.Node) &&
                        !CSharpDebugOnlyCodeHelper.IsCallerInConditionalDebug(methodCall, c.SemanticModel))

                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(SupportedDiagnostics[0],
                            methodCall.Expression.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);
        }
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class DoNotWriteToStandardOutput : DoNotWriteToStandardOutputBase
    {
        private const string DiagnosticId = "S106";
        private const string MessageFormat = "Remove this logging statement.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);
    }
}
