/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.VisualBasic.Rules
{
    [ExportCodeFixProvider(LanguageNames.VisualBasic)]
    public sealed class ArrayDesignatorOnVariableCodeFix : SonarCodeFix
    {
        internal const string Title = "Move the array designator to the type";

        public override ImmutableArray<string> FixableDiagnosticIds => ImmutableArray.Create(ArrayDesignatorOnVariable.DiagnosticId);

        protected override Task RegisterCodeFixesAsync(SyntaxNode root, SonarCodeFixContext context)
        {
            var diagnostic = context.Diagnostics.First();
            var diagnosticSpan = diagnostic.Location.SourceSpan;
            var name = root.FindNode(diagnosticSpan) as ModifiedIdentifierSyntax;

            if (!(name?.Parent is VariableDeclaratorSyntax variableDeclarator) ||
                variableDeclarator.Names.Count != 1)
            {
                return Task.CompletedTask;
            }

            if (!(variableDeclarator.AsClause is SimpleAsClauseSyntax simpleAsClause))
            {
                return Task.CompletedTask;
            }

            context.RegisterCodeFix(
                Title,
                c =>
                {
                    var type = simpleAsClause.Type.WithoutTrivia();
                    var rankSpecifiers = name.ArrayRankSpecifiers.Select(rank => rank.WithoutTrivia());
                    var newType = !(type is ArrayTypeSyntax typeAsArrayType)
                        ? SyntaxFactory.ArrayType(
                                    type,
                                    SyntaxFactory.List(rankSpecifiers))
                        : typeAsArrayType.AddRankSpecifiers(rankSpecifiers.ToArray());

                    newType = newType.WithTriviaFrom(simpleAsClause.Type);

                    var newVariableDeclarator = variableDeclarator
                        .WithNames(SyntaxFactory.SeparatedList(new[]
                        {
                            SyntaxFactory.ModifiedIdentifier(name.Identifier, name.ArrayBounds).WithTriviaFrom(name)
                        }))
                        .WithAsClause(simpleAsClause.WithType(newType));

                    var newRoot = root.ReplaceNode(
                        variableDeclarator,
                        newVariableDeclarator);

                    return Task.FromResult(context.Document.WithSyntaxRoot(newRoot));
                },
                context.Diagnostics);

            return Task.CompletedTask;
        }
    }
}
