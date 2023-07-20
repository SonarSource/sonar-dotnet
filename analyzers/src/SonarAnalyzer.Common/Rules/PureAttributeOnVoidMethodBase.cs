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

namespace SonarAnalyzer.Rules
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
                        c.ReportIssue(Language.GeneratedCodeRecognizer, CreateDiagnostic(Rule, pureAttribute.ApplicationSyntaxReference.GetSyntax().GetLocation()));
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
