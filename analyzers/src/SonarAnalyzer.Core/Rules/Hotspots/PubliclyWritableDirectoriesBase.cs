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

        protected static string[] InsecureEnvironmentVariables { get; }

        private static readonly Regex UserProfile;
        private static readonly Regex LinuxDirectories;
        private static readonly Regex MacDirectories;
        private static readonly Regex WindowsDirectories;
        private static readonly Regex EnvironmentVariables;

        static PubliclyWritableDirectoriesBase()
        {
            var insecureEnvironmentVariables = new[] { "tmp", "temp", "tmpdir" };
            InsecureEnvironmentVariables = insecureEnvironmentVariables;
            UserProfile = new("""^%USERPROFILE%[\\\/]AppData[\\\/]Local[\\\/]Temp""", WindowsAndUnixOptions, RegexConstants.DefaultTimeout);
            LinuxDirectories = new($@"^({LinuxDirs().JoinStr("|", Regex.Escape)})(\/|$)", RegexOptions.Compiled, RegexConstants.DefaultTimeout);
            MacDirectories = new($@"^({MacDirs().JoinStr("|", Regex.Escape)})(\/|$)", WindowsAndUnixOptions, RegexConstants.DefaultTimeout);
            WindowsDirectories = new("""^([a-z]:[\\\/]?|[\\\/][\\\/][^\\\/]+[\\\/]|[\\\/])(windows[\\\/])?te?mp([\\\/]|$)""", WindowsAndUnixOptions, RegexConstants.DefaultTimeout);
            EnvironmentVariables = new($@"^%({insecureEnvironmentVariables.JoinStr("|")})%([\\\/]|$)", WindowsAndUnixOptions, RegexConstants.DefaultTimeout);
        }

        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        private protected abstract bool IsGetTempPathAssignment(TInvocationExpression invocationExpression, KnownType type, string methodName, SemanticModel semanticModel);
        private protected abstract bool IsInsecureEnvironmentVariableRetrieval(TInvocationExpression invocation, KnownType type, string methodName, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected PubliclyWritableDirectoriesBase(IAnalyzerConfiguration configuration) : base(configuration)
        {
            rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);
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
                        && Language.Syntax.StringValue(c.Node, c.Model) is { } stringValue
                        && IsSensitiveDirectoryUsage(stringValue))
                    {
                        c.ReportIssue(rule, c.Node);
                    }
                },
                kinds.ToArray());

            context.RegisterNodeAction(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (IsEnabled(c.Options)
                        && c.Node is TInvocationExpression invocation
                        && (IsGetTempPathAssignment(invocation, KnownType.System_IO_Path, "GetTempPath", c.Model)
                            || IsInsecureEnvironmentVariableRetrieval(invocation, KnownType.System_Environment, "GetEnvironmentVariable", c.Model)))
                    {
                        c.ReportIssue(rule, c.Node);
                    }
                },
                Language.SyntaxKind.InvocationExpression);
        }

        private static bool IsSensitiveDirectoryUsage(string directory) =>
            WindowsDirectories.SafeIsMatch(directory)
                || MacDirectories.SafeIsMatch(directory)
                || LinuxDirectories.SafeIsMatch(directory)
                || EnvironmentVariables.SafeIsMatch(directory)
                || UserProfile.SafeIsMatch(directory);

        private static string[] LinuxDirs() => new[]
            {
                "/dev/mqueue",
                "/run/lock",
                "/var/run/lock",
            };

        private static string[] MacDirs() => new[]
            {
                "/var/tmp",
                "/usr/tmp",
                "/dev/shm",
                "/library/caches",
                "/users/shared",
                "/private/tmp",
                "/private/var/tmp",
            };
    }
}
