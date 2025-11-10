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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PureAttributeOnVoidMethod : PureAttributeOnVoidMethodBase<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override void Initialize(SonarAnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterNodeAction(
                c =>
                {
                    if ((LocalFunctionStatementSyntaxWrapper)c.Node is var localFunction
                        && localFunction.AttributeLists.SelectMany(x => x.Attributes).Any(IsPureAttribute)
                        && InvalidPureDataAttributeUsage((IMethodSymbol)c.Model.GetDeclaredSymbol(c.Node)) is { } pureAttribute)
                    {
                        c.ReportIssue(Rule, pureAttribute.ApplicationSyntaxReference.GetSyntax());
                    }
                },
                SyntaxKindEx.LocalFunctionStatement);
        }

        private static bool IsPureAttribute(AttributeSyntax attribute) =>
            attribute.Name.GetIdentifier() is { ValueText: "Pure" or "PureAttribute" };
    }
}
