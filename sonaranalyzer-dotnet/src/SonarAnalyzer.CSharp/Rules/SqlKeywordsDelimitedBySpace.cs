/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class SqlKeywordsDelimitedBySpace : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2857";
        private const string MessageFormat = "Add a space before '{0}'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly IList<NameSyntax> SqlNamespaces = new List<NameSyntax>()
        {
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("Microsoft", "EntityFrameworkCore"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("ServiceStack", "OrmLite"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "SqlClient"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "SQLite"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "SqlServerCe"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "Entity"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "Odbc"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("System", "Data", "OracleClient"),
            CSharpSyntaxHelper.BuildQualifiedNameSyntax("Microsoft", "Data", "Sqlite"),
            SyntaxFactory.IdentifierName("Dapper"),
            SyntaxFactory.IdentifierName("NHibernate"),
            SyntaxFactory.IdentifierName("PetaPoco")
        };

        private static readonly IList<string> SqlKeywords = new List<string>()
        {
            "BULK INSERT",
            "SELECT",
            "DELETE",
            "UPDATE",
            "INSERT",
            "UPDATETEXT",
            "MERGE",
            "WRITETEXT",
            "READTEXT"
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var namespaceDeclaration = (NamespaceDeclarationSyntax)c.Node;
                    var compilationUnit = namespaceDeclaration.Parent as CompilationUnitSyntax;
                    if (compilationUnit == null ||
                        (!HasSqlNamespace(compilationUnit.Usings) &&
                        !HasSqlNamespace(namespaceDeclaration.Usings)))
                    {
                        return;
                    }
                    var visitor = new CSharpStringLiteralWalker(c);
                    foreach (var member in namespaceDeclaration.Members)
                    {
                        visitor.SafeVisit(member);
                    }
            },
            SyntaxKind.NamespaceDeclaration);
        }

        private bool HasSqlNamespace(SyntaxList<UsingDirectiveSyntax> usings) =>
            usings.Select(usingDirective => usingDirective.Name)
                .Any(name => SqlNamespaces.Any(sn => SyntaxFactory.AreEquivalent(name, sn)));

        private class CSharpStringLiteralWalker : CSharpSyntaxWalker
        {
            private readonly SyntaxNodeAnalysisContext context;

            public CSharpStringLiteralWalker(SyntaxNodeAnalysisContext context)
            {
                this.context = context;
            }

            public override void VisitLiteralExpression(LiteralExpressionSyntax node)
            {
                if (!node.IsKind(SyntaxKind.StringLiteralExpression) ||
                    !(node.Parent is BinaryExpressionSyntax parentConcatenation) ||
                    !parentConcatenation.IsKind(SyntaxKind.AddExpression) ||
                    node != parentConcatenation.Right)
                {
                    return;
                }

                var firstStringInConcatenation = GetFirstStringFromConcatenation(parentConcatenation);
                var beforeCurrentString = GetPenultimateString(parentConcatenation);
                if (!firstStringInConcatenation.IsKind(SyntaxKind.StringLiteralExpression) ||
                    !StartsWithSqlKeyword(((LiteralExpressionSyntax)firstStringInConcatenation).Token.ValueText.Trim()) ||
                    !beforeCurrentString.IsKind(SyntaxKind.StringLiteralExpression))
                {
                    return;
                }

                var previousStringLastChar = ((LiteralExpressionSyntax)beforeCurrentString).Token.ValueText.ToCharArray().Last();
                var currentStringLastChar = node.Token.ValueText[0];
                if (IsKnownCharacter(previousStringLastChar) && IsKnownCharacter(currentStringLastChar))
                {
                    var nodeFirstToken = node.Token.ValueText.Trim().Split(' ').First();
                    this.context.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation(), nodeFirstToken));
                }
            }

            // Given a concatenation "a" + "b" + "c", this will return the "b" syntax node (one before last)
            private static SyntaxNode GetPenultimateString(BinaryExpressionSyntax concatenation)
            {
                var penultimateElement = concatenation.Left;
                while (penultimateElement is BinaryExpressionSyntax previousAddition &&
                    previousAddition.IsKind(SyntaxKind.AddExpression))
                {
                    penultimateElement = previousAddition.Right;
                }
                return penultimateElement;
            }

            // Given a concatenation "a" + "b" + "c", this will return the "a" syntax node
            private static SyntaxNode GetFirstStringFromConcatenation(BinaryExpressionSyntax concatenation)
            {
                var firstElement = concatenation.Left;
                while (firstElement is BinaryExpressionSyntax toTheLeftAddition &&
                    firstElement.IsKind(SyntaxKind.AddExpression))
                {
                    firstElement = toTheLeftAddition.Left;
                }
                return firstElement;
            }

            private static bool StartsWithSqlKeyword(string firstString) =>
                SqlKeywords.Any(s => firstString.StartsWith(s, StringComparison.OrdinalIgnoreCase));

            /**
             * The '@' symbol is used for named parameters.
             * The '*' is a widely used wildcard.
             * We ignore other non-alphanumeric characters (e.g. '>','=') to avoid false positives.
             */
            private static bool IsKnownCharacter(char c) => char.IsLetterOrDigit(c) || c == '@' || c == '*';
        }
    }
}
