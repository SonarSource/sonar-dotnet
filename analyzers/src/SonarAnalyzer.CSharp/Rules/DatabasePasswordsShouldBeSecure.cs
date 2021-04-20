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
    public sealed class DatabasePasswordsShouldBeSecure : TrackerHotspotDiagnosticAnalyzer<SyntaxKind>
    {
        private const string DiagnosticId = "S2115";
        private const string MessageFormat = "Use a secure password when connecting to this database.";

        private static readonly ISet<string> Sanitizers = new HashSet<string>
        {
            "Integrated Security",
            "Trusted_Connection"
        };

        private readonly MemberDescriptor[] trackedInvocations =
        {
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, "UseSqlServer"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_SqlServerDbContextOptionsExtensions, "UseSqlServer"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, "UseMySQL"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_MySQLDbContextOptionsExtensions, "UseMySQL"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, "UseSqlite"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_SqliteDbContextOptionsBuilderExtensions, "UseSqlite"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, "UseOracle"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_OracleDbContextOptionsExtensions, "UseOracle"),
            // for UseNpgsql,the namespaces are different in .NET Core 3.1 and .NET 5
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_DbContextOptionsBuilder, "UseNpgsql"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsExtensions, "UseNpgsql"),
            new MemberDescriptor(KnownType.Microsoft_EntityFrameworkCore_NpgsqlDbContextOptionsBuilderExtensions, "UseNpgsql"),
        };

        public DatabasePasswordsShouldBeSecure()
            : base(AnalyzerConfiguration.AlwaysEnabled, DiagnosticId, MessageFormat) { }

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        protected override void Initialize(TrackerInput input)
        {
            var inv = Language.Tracker.Invocation;
            inv.Track(input, inv.MatchMethod(trackedInvocations), HasEmptyPasswordArgument());
        }

        private static TrackerBase<SyntaxKind, InvocationContext>.Condition HasEmptyPasswordArgument() =>
            context =>
            {
                var argumentList = ((InvocationExpressionSyntax)context.Node).ArgumentList.Arguments;
                return ConnectionStringArgument(argumentList)?.Expression switch
                {
                    LiteralExpressionSyntax literal => IsVulnerable(literal.Token.ValueText) && !HasSanitizers(literal.Token.ValueText),
                    InterpolatedStringExpressionSyntax interpolatedString => HasEmptyPasswordAndNoSanitizers(interpolatedString),
                    BinaryExpressionSyntax binaryExpression when binaryExpression.IsKind(SyntaxKind.AddExpression) => HasEmptyPasswordAndNoSanitizers(binaryExpression),
                    _ => false
                };
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
