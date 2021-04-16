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
            // the namespaces are different in .NET Core 3.1 and .NET 5
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

        private static bool HasEmptyPasswordArgument(SeparatedSyntaxList<ArgumentSyntax> argumentList) =>
            ConnectionStringArgument(argumentList)?.Expression switch
            {
                LiteralExpressionSyntax literal when literal.IsKind(SyntaxKind.StringLiteralExpression) => IsVulnerable(literal.Token.ValueText) && !HasSanitizers(literal.Token.ValueText),
                InterpolatedStringExpressionSyntax interpolatedString => HasEmptyPasswordAndNoSanitizers(interpolatedString),
                BinaryExpressionSyntax binaryExpression when binaryExpression.IsKind(SyntaxKind.AddExpression) => HasEmptyPasswordAndNoSanitizers(binaryExpression),
                _ => false
            };

        // First search a named argument, then search literals, then fallback on the first argument (for constant propagation check).
        // This is an easy way to support explicit extension method invocation.
        private static ArgumentSyntax ConnectionStringArgument(SeparatedSyntaxList<ArgumentSyntax> argumentList) =>
            // Where(cond).First() is more efficient than First(cond)
            argumentList.Where(a => a.NameColon?.Name.Identifier.ValueText == "connectionString").FirstOrDefault()
                ?? argumentList.Where(a => a.Expression.IsAnyKind(SyntaxKind.StringLiteralExpression, SyntaxKind.InterpolatedStringExpression, SyntaxKind.AddExpression)).FirstOrDefault()
                ?? argumentList.FirstOrDefault();

        // For both interpolated strings and concatenation chain, it's easier to search in the string representation of the tree, rather than doing string searches for each individual
        // string token inside.
        private static bool HasEmptyPasswordAndNoSanitizers(ExpressionSyntax expression)
        {
            var toString = expression.ToString();
            return IsVulnerable(toString) && !HasSanitizers(toString);
        }

        private static bool IsVulnerable(string connectionString) =>
            connectionString.EndsWith("Password=")
            || connectionString.Contains("Password=;")
            // this is an edge case, for a string interpolation or concatenation the toString() will contain the ending "
            // we prefer to keep it like this for the simplicity of the implementation
            || connectionString.EndsWith("Password=\"");

        private static bool HasSanitizers(string connectionString) => Sanitizers.Any(connectionString.Contains);
    }
}
