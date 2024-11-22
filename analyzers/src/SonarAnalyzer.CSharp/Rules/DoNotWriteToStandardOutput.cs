/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DoNotWriteToStandardOutput : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S106";
        private const string MessageFormat = "Remove this logging statement.";

        protected static DiagnosticDescriptor Rule =>
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

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
                        c.ReportIssue(Rule, methodCall.Expression);
                    }
                },
                SyntaxKind.InvocationExpression);
    }
}
