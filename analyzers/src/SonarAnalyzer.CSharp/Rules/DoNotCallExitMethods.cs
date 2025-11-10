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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.CSharp.Rules
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
                      .Any(x => x?.Kind() is
                        SyntaxKindEx.LocalFunctionStatement
                        or SyntaxKind.ParenthesizedLambdaExpression
                        or SyntaxKind.SimpleLambdaExpression
                        or SyntaxKind.AnonymousMethodExpression)
                : !invocationSyntax.Ancestors().OfType<BaseMethodDeclarationSyntax>().Where(x => x.GetIdentifierOrDefault()?.ValueText == "Main")
                      .Select(m => semanticModel.GetDeclaredSymbol(m))
                      .Select(s => s.IsMainMethod())
                      .FirstOrDefault();
    }
}
