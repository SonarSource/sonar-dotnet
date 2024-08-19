/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DoNotCallExitMethods : DoNotCallMethodsCSharpBase
    {
        private const string DiagnosticId = "S1147";
        protected override string MessageFormat => "Remove this call to '{0}' or ensure it is really required.";

        protected override IEnumerable<MemberDescriptor> CheckedMethods { get; } = new List<MemberDescriptor>
        {
            new(KnownType.System_Environment, "Exit"),
            new(KnownType.System_Windows_Forms_Application, "Exit")
        };

        public DoNotCallExitMethods() : base(DiagnosticId) { }

        protected override bool IsInValidContext(InvocationExpressionSyntax invocationSyntax, SemanticModel semanticModel) =>
            // Do not report if call is inside Main or is a TopLevelStatement.
            invocationSyntax.Ancestors().OfType<GlobalStatementSyntax>().Any()
                ? invocationSyntax.Ancestors()
                      .Any(x => x.IsAnyKind(SyntaxKindEx.LocalFunctionStatement, SyntaxKind.ParenthesizedLambdaExpression, SyntaxKind.SimpleLambdaExpression, SyntaxKind.AnonymousMethodExpression))
                : !invocationSyntax.Ancestors().OfType<BaseMethodDeclarationSyntax>().Where(x => x.GetIdentifierOrDefault()?.ValueText == "Main")
                      .Select(m => semanticModel.GetDeclaredSymbol(m))
                      .Select(s => s.IsMainMethod())
                      .FirstOrDefault();
    }
}
