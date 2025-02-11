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

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DisposableReturnedFromUsing : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2997";
        private const string MessageFormat = "Remove the 'using' statement; it will cause automatic disposal of {0}.";

        private static readonly DiagnosticDescriptor rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterNodeAction(
                c =>
                {
                    var usingStatement = (UsingStatementSyntax) c.Node;
                    var declaration = usingStatement.Declaration;
                    var symbolsDeclaredInUsing = new HashSet<ISymbol>();
                    if (declaration != null)
                    {
                        symbolsDeclaredInUsing =
                            declaration.Variables.Select(syntax => c.Model.GetDeclaredSymbol(syntax))
                                .WhereNotNull()
                                .ToHashSet();
                    }
                    else if (usingStatement.Expression is AssignmentExpressionSyntax assignment)
                    {
                        if (!(assignment.Left is IdentifierNameSyntax identifierName))
                        {
                            return;
                        }
                        var symbol = c.Model.GetSymbolInfo(identifierName).Symbol;
                        if (symbol == null)
                        {
                            return;
                        }
                        symbolsDeclaredInUsing = new HashSet<ISymbol> { symbol };
                    }

                    CheckReturns(c, usingStatement.UsingKeyword, usingStatement.Statement, symbolsDeclaredInUsing);
                },
                SyntaxKind.UsingStatement);

            context.RegisterNodeAction(
                c =>
                {
                    var localDeclarationStatement = (LocalDeclarationStatementSyntax)c.Node;
                    var usingKeyword = localDeclarationStatement.UsingKeyword();
                    if (!usingKeyword.IsKind(SyntaxKind.UsingKeyword))
                    {
                        return;
                    }

                    var declaredSymbols = localDeclarationStatement.Declaration.Variables
                                .Select(syntax => c.Model.GetDeclaredSymbol(syntax))
                                .WhereNotNull()
                                .ToHashSet();

                    CheckReturns(c, usingKeyword, localDeclarationStatement.Parent, declaredSymbols);
                },
                SyntaxKind.LocalDeclarationStatement);
        }

        private static void CheckReturns(SonarSyntaxNodeReportingContext c, SyntaxToken usingKeyword, SyntaxNode body, HashSet<ISymbol> declaredSymbols)
        {
            if (declaredSymbols.Count == 0)
            {
                return;
            }

            var returnedSymbols = GetReturnedSymbols(body, c.Model);
            returnedSymbols.IntersectWith(declaredSymbols);

            if (returnedSymbols.Any())
            {
                c.ReportIssue(rule, usingKeyword, returnedSymbols.Select(s => $"'{s.Name}'").OrderBy(s => s).JoinAnd());
            }
        }

        private static ISet<ISymbol> GetReturnedSymbols(SyntaxNode body, SemanticModel semanticModel)
        {
            var enclosingSymbol = semanticModel.GetEnclosingSymbol(body.SpanStart);

            return body.DescendantNodesAndSelf()
                .OfType<ReturnStatementSyntax>()
                .Where(ret => semanticModel.GetEnclosingSymbol(ret.SpanStart).Equals(enclosingSymbol))
                .Select(ret => ret.Expression)
                .OfType<IdentifierNameSyntax>()
                .Select(identifier => semanticModel.GetSymbolInfo(identifier).Symbol)
                .WhereNotNull()
                .ToHashSet();
        }
    }
}
