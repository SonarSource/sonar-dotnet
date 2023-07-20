/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Xml.XPath;
using SonarAnalyzer.Json;
using SonarAnalyzer.Json.Parsing;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class DatabasePasswordsShouldBeSecure : TrackerHotspotDiagnosticAnalyzer<SyntaxKind>
    {
        private const string DiagnosticId = "S2115";
        private const string MessageFormat = "Use a secure password when connecting to this database.";

        private static readonly Regex Sanitizers = new(@"((integrated[_\s]security)|(trusted[_\s]connection))=(sspi|yes|true)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private static readonly MemberDescriptor[] TrackedInvocations =
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

        protected override ILanguageFacade<SyntaxKind> Language => CSharpFacade.Instance;

        public DatabasePasswordsShouldBeSecure()
            : base(AnalyzerConfiguration.AlwaysEnabled, DiagnosticId, MessageFormat) { }

        protected override void Initialize(TrackerInput input)
        {
            var inv = Language.Tracker.Invocation;
            inv.Track(input, inv.MatchMethod(TrackedInvocations), HasEmptyPasswordArgument());
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            base.Initialize(context);
            context.RegisterCompilationAction(CheckWebConfig);
            context.RegisterCompilationAction(CheckAppSettings);
        }

        private void CheckWebConfig(SonarCompilationReportingContext c)
        {
            foreach (var fullPath in c.WebConfigFiles())
            {
                var webConfig = File.ReadAllText(fullPath);
                if (webConfig.Contains("<connectionStrings>") && XmlHelper.ParseXDocument(webConfig) is { } doc)
                {
                    ReportEmptyPassword(c, doc, fullPath);
                }
            }
        }

        private void CheckAppSettings(SonarCompilationReportingContext c)
        {
            foreach (var fullPath in c.AppSettingsFiles())
            {
                CheckAppSettingJson(c, fullPath);
            }
        }

        private void CheckAppSettingJson(SonarCompilationReportingContext c, string fullPath)
        {
            var appSettings = File.ReadAllText(fullPath);
            if (appSettings.Contains("\"ConnectionStrings\"") && JsonNode.FromString(appSettings) is { } json)
            {
                ReportEmptyPassword(c, json, fullPath);
            }
        }

        private void ReportEmptyPassword(SonarCompilationReportingContext c, XDocument doc, string webConfigPath)
        {
            foreach (var addAttribute in doc.XPathSelectElements("configuration/connectionStrings/add"))
            {
                if (addAttribute.Attribute("connectionString") is { } connectionString
                    && IsVulnerable(connectionString.Value)
                    && !HasSanitizers(connectionString.Value)
                    && connectionString.CreateLocation(webConfigPath) is { } location)
                {
                    c.ReportIssue(CreateDiagnostic(Rule, location));
                }
            }
        }

        private void ReportEmptyPassword(SonarCompilationReportingContext c, JsonNode doc, string appSettingsPath)
        {
            if (doc.TryGetPropertyNode("ConnectionStrings", out var connectionStrings) && connectionStrings.Kind == Kind.Object)
            {
                foreach (var key in connectionStrings.Keys)
                {
                    if (connectionStrings[key] is { Kind: Kind.Value, Value: string value } connectionStringNode && IsVulnerable(value))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, connectionStringNode.ToLocation(appSettingsPath)));
                    }
                }
            }
        }

        private static TrackerBase<SyntaxKind, InvocationContext>.Condition HasEmptyPasswordArgument() =>
            context =>
            {
                var argumentList = ((InvocationExpressionSyntax)context.Node).ArgumentList.Arguments;
                return ConnectionStringArgument(argumentList)?.Expression switch
                {
                    LiteralExpressionSyntax literal => IsVulnerable(literal.Token.ValueText) && !HasSanitizers(literal.Token.ValueText),
                    InterpolatedStringExpressionSyntax interpolatedString => HasEmptyPasswordAndNoSanitizers(interpolatedString),
                    BinaryExpressionSyntax binaryExpression => HasEmptyPasswordAndNoSanitizers(binaryExpression),
                    _ => false
                };
            };

        // First search a named argument, then search literals, then fallback on the first argument (for constant propagation check).
        // This is an easy way to support explicit extension method invocation.
        private static ArgumentSyntax ConnectionStringArgument(SeparatedSyntaxList<ArgumentSyntax> argumentList) =>
            // Where(cond).First() is more efficient than First(cond)
            argumentList.Where(a => a.NameColon?.Name.Identifier.ValueText == "connectionString").FirstOrDefault()
                ?? argumentList.Where(a => a.Expression.IsAnyKind(
                    SyntaxKind.StringLiteralExpression,
                    SyntaxKind.InterpolatedStringExpression,
                    SyntaxKind.AddExpression)).FirstOrDefault()
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
            || connectionString.EndsWith("Password=\"")
            // raw string literals
            || connectionString.EndsWith("Password=\"\"\"");

        private static bool HasSanitizers(string connectionString) =>
            Sanitizers.IsMatch(connectionString);
    }
}
