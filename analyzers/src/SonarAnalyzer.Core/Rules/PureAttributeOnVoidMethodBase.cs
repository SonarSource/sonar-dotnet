/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
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

namespace SonarAnalyzer.Core.Rules
{
    public abstract class PureAttributeOnVoidMethodBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        protected const string DiagnosticId = "S3603";

        protected override string MessageFormat => "Remove the 'Pure' attribute or change the method to return a value.";

        protected PureAttributeOnVoidMethodBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    if (InvalidPureDataAttributeUsage((IMethodSymbol)c.Symbol) is { } pureAttribute)
                    {
                        c.ReportIssue(Language.GeneratedCodeRecognizer, Rule, pureAttribute.ApplicationSyntaxReference.GetSyntax());
                    }
                },
                SymbolKind.Method);

        protected static AttributeData InvalidPureDataAttributeUsage(IMethodSymbol method) =>
            NoOutParameters(method)
            && (method.ReturnsVoid || ReturnsTask(method))
            && GetPureAttribute(method) is { } pureAttribute
            ? pureAttribute
            : null;

        private static bool NoOutParameters(IMethodSymbol method) =>
            method.Parameters.All(p => p.RefKind == RefKind.None || p.RefKind == RefKindEx.In);

        private static bool ReturnsTask(IMethodSymbol method) =>
            method.ReturnType.Is(KnownType.System_Threading_Tasks_Task);

        private static AttributeData GetPureAttribute(IMethodSymbol method) =>
            method.GetAttributes().FirstOrDefault(a => a.AttributeClass.Is(KnownType.System_Diagnostics_Contracts_PureAttribute));
    }
}
