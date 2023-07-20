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

using System.Text.RegularExpressions;

namespace SonarAnalyzer.Rules
{
    public abstract class PubliclyWritableDirectoriesBase<TSyntaxKind, TInvocationExpression> : HotspotDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TInvocationExpression : SyntaxNode
    {
        protected const string DiagnosticId = "S5443";
        private const string MessageFormat = "Make sure publicly writable directories are used safely here.";
        private const RegexOptions WindowsAndUnixOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        private readonly string[] linuxDirs =
        {
            "/dev/mqueue",
            "/run/lock",
            "/var/run/lock",
        };
        private readonly string[] macDirs =
        {
            "/var/tmp",
            "/usr/tmp",
            "/dev/shm",
            "/library/caches",
            "/users/shared",
            "/private/tmp",
            "/private/var/tmp",
        };
        private readonly Regex userProfile = new(@"^%USERPROFILE%[\\\/]AppData[\\\/]Local[\\\/]Temp", WindowsAndUnixOptions);
        private readonly Regex linuxDirectories;
        private readonly Regex macDirectories;
        private readonly Regex windowsDirectories = new(@"^([a-z]:[\\\/]?|[\\\/][\\\/][^\\\/]+[\\\/]|[\\\/])(windows[\\\/])?te?mp([\\\/]|$)", WindowsAndUnixOptions);
        private readonly Regex environmentVariables;
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        private protected abstract bool IsGetTempPathAssignment(TInvocationExpression invocationExpression, KnownType type, string methodName, SemanticModel semanticModel);
        private protected abstract bool IsInsecureEnvironmentVariableRetrieval(TInvocationExpression invocation, KnownType type, string methodName, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected string[] InsecureEnvironmentVariables { get; } = { "tmp", "temp", "tmpdir" };

        protected PubliclyWritableDirectoriesBase(IAnalyzerConfiguration configuration) : base(configuration)
        {
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);
            linuxDirectories = new Regex($@"^({linuxDirs.JoinStr("|", x => Regex.Escape(x))})(\/|$)", RegexOptions.Compiled);
            macDirectories = new Regex($@"^({macDirs.JoinStr("|", x => Regex.Escape(x))})(\/|$)", WindowsAndUnixOptions);
            environmentVariables = new Regex($@"^%({InsecureEnvironmentVariables.JoinStr("|")})%([\\\/]|$)", WindowsAndUnixOptions);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            var kinds = Language.SyntaxKind.StringLiteralExpressions.ToList();
            kinds.Add(Language.SyntaxKind.InterpolatedStringExpression);

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (IsEnabled(c.Options)
                        && Language.Syntax.StringValue(c.Node, c.SemanticModel) is { } stringValue
                        && IsSensitiveDirectoryUsage(stringValue))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, c.Node.GetLocation()));
                    }
                },
                kinds.ToArray());

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (IsEnabled(c.Options)
                        && c.Node is TInvocationExpression invocation
                        && (IsGetTempPathAssignment(invocation, KnownType.System_IO_Path, "GetTempPath", c.SemanticModel)
                            || IsInsecureEnvironmentVariableRetrieval(invocation, KnownType.System_Environment, "GetEnvironmentVariable", c.SemanticModel)))
                    {
                        c.ReportIssue(CreateDiagnostic(rule, c.Node.GetLocation()));
                    }
                },
                Language.SyntaxKind.InvocationExpression);
        }

        private bool IsSensitiveDirectoryUsage(string directory) =>
            windowsDirectories.IsMatch(directory)
            || macDirectories.IsMatch(directory)
            || linuxDirectories.IsMatch(directory)
            || environmentVariables.IsMatch(directory)
            || userProfile.IsMatch(directory);
    }
}
