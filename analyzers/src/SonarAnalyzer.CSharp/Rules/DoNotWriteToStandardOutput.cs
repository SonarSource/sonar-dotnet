/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    // This base class is only there to avoid duplication between the implementation of S106 and S2228
    public abstract class DoNotWriteToStandardOutputBase : SonarDiagnosticAnalyzer
    {
        protected abstract DiagnosticDescriptor Rule { get; }

        private static readonly ISet<string> BannedConsoleMembers = new HashSet<string> { "WriteLine", "Write" };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
            ImmutableArray.Create(Rule);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
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
                        !c.Node.IsInDebugBlock() &&
                        !CSharpDebugOnlyCodeHelper.IsCallerInConditionalDebug(methodCall, c.SemanticModel))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, methodCall.Expression.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);
    }

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotWriteToStandardOutput : DoNotWriteToStandardOutputBase
    {
        private const string DiagnosticId = "S106";
        private const string MessageFormat = "Remove this logging statement.";

        protected override DiagnosticDescriptor Rule =>
            DescriptorFactory.Create(DiagnosticId, MessageFormat);
    }
}
