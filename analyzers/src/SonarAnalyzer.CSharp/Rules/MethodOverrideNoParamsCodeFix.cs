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

using Microsoft.CodeAnalysis.CodeFixes;

namespace SonarAnalyzer.Rules.CSharp
{
    [ExportCodeFixProvider(LanguageNames.CSharp)]
    public sealed class MethodOverrideNoParamsCodeFix : SonarCodeFix
    {
        internal const string Title = "Add the 'params' modifier";
        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(MethodOverrideNoParams.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var parameter = (ParameterSyntax)root.FindNode(diagnosticSpan);

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var newParameter = parameter.WithModifiers(
                        parameter.Modifiers.Add(
                            SyntaxFactory.Token(SyntaxKind.ParamsKeyword)));
                    var newRoot = root.ReplaceNode(
                        parameter,
                        newParameter.WithTriviaFrom(parameter));
                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}
