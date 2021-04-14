/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using System.Collections.Immutable;
using System;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using System.Collections.Generic;
using SonarAnalyzer.Extensions;
using System.Linq;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class DatabasePasswordsShouldBeSecure : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2115";
        private const string MessageFormat = "Use a secure password when connecting to this database.";

        private static readonly ISet<string> Sanitizers = new HashSet<string>
        {
            "Integrated Security",
            "Trusted_Connection"
        };

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        private static IDictionary<string, ImmutableArray<KnownType>> NamesWithTypes = new Dictionary<string, ImmutableArray<KnownType>>()
        {
            { "UseSqlServer", ImmutableArray.Create(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, KnownType.Microsoft_EntityFrameworkCore_SqlServerDbContextOptionsExtensions) },
            { "UseNpgsql", ImmutableArray.Create(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, KnownType.Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsExtensions, KnownType.Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsBuilderExtensions) },
            { "UseMySQL", ImmutableArray.Create(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, KnownType.Microsoft_EntityFrameworkCore_MySQLDbContextOptionsExtensions)  },
            { "UseSqlite", ImmutableArray.Create(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, KnownType.Microsoft_EntityFrameworkCore_SqliteDbContextOptionsBuilderExtensions)  },
            { "UseOracle", ImmutableArray.Create(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, KnownType.Microsoft_EntityFrameworkCore_OracleDbContextOptionsExtensions)  },
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(c =>
                {
                    var invocation = (InvocationExpressionSyntax)c.Node;
                    if (IsConnectionStringInvocation(invocation, c.SemanticModel)
                        && HasEmptyPasswordArgument(invocation.ArgumentList.Arguments))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, invocation.GetLocation()));
                    }
                },
                SyntaxKind.InvocationExpression);

        private static bool IsConnectionStringInvocation(InvocationExpressionSyntax invocation, SemanticModel semanticModel) =>
            invocation.Expression is MemberAccessExpressionSyntax memberAccess
            && memberAccess.GetName() is { } name
            && NamesWithTypes.ContainsKey(name)
            && IsInvocationOn(memberAccess, semanticModel, NamesWithTypes[name]);

        private static bool IsInvocationOn(MemberAccessExpressionSyntax memberAccess, SemanticModel semanticModel, ImmutableArray<KnownType> knownTypes) =>
            semanticModel.GetSymbolInfo(memberAccess).Symbol is { } symbol
            && symbol.ContainingType.DerivesFromAny(knownTypes);

        private static bool HasEmptyPasswordArgument(SeparatedSyntaxList<ArgumentSyntax> argumentList)
        {
            var connectionStringArgument = ConnectionStringArgument(argumentList)?.Expression;
            if (connectionStringArgument == null)
            {
                return false;
            }

            if (connectionStringArgument is LiteralExpressionSyntax literal && literal.IsKind(SyntaxKind.StringLiteralExpression))
            {
                return IsVulnerable(literal.Token.ValueText) && !HasSanitizers(literal.Token.ValueText);
            }
            else if (connectionStringArgument is InterpolatedStringExpressionSyntax interpolatedString && interpolatedString.Contents.Count > 0)
            {
                if (interpolatedString.Contents.Last() is InterpolatedStringTextSyntax interpolatedStringText && IsVulnerable(interpolatedStringText.TextToken.ValueText))
                {
                    return !HasSanitizers(interpolatedString.Contents.ToString());
                }
                else
                {
                    var contents = interpolatedString.Contents.ToString();
                    return IsVulnerable(contents) && !HasSanitizers(contents);
                }
            }
            else if (connectionStringArgument is BinaryExpressionSyntax binaryExpression && binaryExpression.IsKind(SyntaxKind.AddExpression))
            {
                if (binaryExpression.Right is LiteralExpressionSyntax literal2 && literal2.IsKind(SyntaxKind.StringLiteralExpression) && IsVulnerable(literal2.Token.ValueText))
                {
                    return !HasSanitizers(binaryExpression.ToString());
                }
                else
                {
                    var concatenation = binaryExpression.ToString();
                    return IsVulnerable(concatenation) && !HasSanitizers(concatenation);
                }
            }
            return false;
        }

        private static bool IsVulnerable(string connectionString) => (connectionString.EndsWith("Password=") || connectionString.Contains("Password=;"));

        private static bool HasSanitizers(string connectionString) => Sanitizers.Any(connectionString.Contains);

        private static ArgumentSyntax ConnectionStringArgument(SeparatedSyntaxList<ArgumentSyntax> argumentList)
        {
            ArgumentSyntax result = null;
            if (argumentList.Count > 0)
            {
                // Where(cond).First() is more efficient than First(cond)
                var namedArgument = argumentList.Where(a => a.NameColon?.Name.Identifier.ValueText == "connectionString").FirstOrDefault();
                result = namedArgument
                    ?? argumentList.Where(a => a.Expression.IsAnyKind(SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression, SyntaxKind.AddExpression)).FirstOrDefault()
                    ?? argumentList.First();
            }
            return result;
        }
    }
}
