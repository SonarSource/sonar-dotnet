/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using SonarAnalyzer.ShimLayer.CSharp;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class ReturnEmptyCollectionInsteadOfNull : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1168";
        private const string MessageFormat = "Return an empty collection instead of null.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<KnownType> CollectionTypes = new HashSet<KnownType>
        {
            KnownType.System_Collections_IEnumerable,
            KnownType.System_Array
        };

        private static readonly ISet<KnownType> IgnoredTypes = new HashSet<KnownType>
        {
            KnownType.System_Xml_XmlNode
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(ReportIfReturnsNull,
                SyntaxKind.MethodDeclaration, SyntaxKind.PropertyDeclaration);
        }

        private static void ReportIfReturnsNull(SyntaxNodeAnalysisContext context)
        {
            if (!IsReturningCollection(context))
            {
                return;
            }

            var propertyDeclaration = context.Node as PropertyDeclarationSyntax;
            var methodDeclaration = context.Node as MethodDeclarationSyntax;

            var propertyGetAccessor = propertyDeclaration?.AccessorList?.Accessors
                .FirstOrDefault(accessor => accessor.IsKind(SyntaxKind.GetAccessorDeclaration));

            var expressionBody = methodDeclaration?.ExpressionBody
                ?? propertyDeclaration?.ExpressionBody
                ?? propertyGetAccessor?.ExpressionBody();

            if (expressionBody != null)
            {
                var arrowedNullLiteral = GetNullLiteralOrDefault(expressionBody);
                if (arrowedNullLiteral != null)
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, arrowedNullLiteral.GetLocation()));
                }

                return;
            }

            var methodBody = methodDeclaration?.Body
                ?? propertyGetAccessor?.Body;
            if (methodBody != null)
            {
                var returnNullStatements = GetReturnNullStatements(methodBody)
                    .Select(returnStatement => returnStatement.GetLocation())
                    .ToList();
                if (returnNullStatements.Count > 0)
                {
                    context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, returnNullStatements[0],
                        additionalLocations: returnNullStatements.Skip(1)));
                }
            }
        }

        private static bool IsReturningCollection(SyntaxNodeAnalysisContext context)
        {
            var symbol = context.SemanticModel.GetDeclaredSymbol(context.Node);

            var methodSymbol = (symbol as IPropertySymbol)?.GetMethod ?? symbol as IMethodSymbol;

            return methodSymbol != null &&
                !methodSymbol.ReturnType.Is(KnownType.System_String) &&
                methodSymbol.ReturnType.DerivesOrImplementsAny(CollectionTypes) &&
                !methodSymbol.ReturnType.DerivesFromAny(IgnoredTypes);
        }

        private static LiteralExpressionSyntax GetNullLiteralOrDefault(ArrowExpressionClauseSyntax expressionBody)
        {
            return expressionBody.Expression.IsKind(SyntaxKind.NullLiteralExpression)
                ? (LiteralExpressionSyntax)expressionBody.Expression
                : null;
        }

        private static IEnumerable<ReturnStatementSyntax> GetReturnNullStatements(BlockSyntax methodBlock)
        {
            return methodBlock.DescendantNodes()
                .OfType<ReturnStatementSyntax>()
                .Where(returnStatement =>
                    returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression) &&
                    returnStatement.FirstAncestorOrSelf<ParenthesizedLambdaExpressionSyntax>() == null);
        }
    }
}
