/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Core.Rules;

public abstract class PubliclyWritableDirectoriesBase<TSyntaxKind, TInvocationExpression> : SonarDiagnosticAnalyzer
    where TSyntaxKind : struct
    where TInvocationExpression : SyntaxNode
{
    protected const string DiagnosticId = "S5443";
    private const string MessageFormat = "Use a directory that is not publicly writable.";
    private const RegexOptions WindowsAndUnixOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;

    protected static readonly string[] InsecureEnvironmentVariables = ["tmp", "temp", "tmpdir"];

    private static readonly Regex UserProfile = new("""^%USERPROFILE%[\\\/]AppData[\\\/]Local[\\\/]Temp""", WindowsAndUnixOptions, Constants.DefaultRegexTimeout);
    private static readonly Regex LinuxDirectories = new($@"^({LinuxDirs().JoinStr("|", Regex.Escape)})(\/|$)", RegexOptions.Compiled, Constants.DefaultRegexTimeout);
    private static readonly Regex MacDirectories = new($@"^({MacDirs().JoinStr("|", Regex.Escape)})(\/|$)", WindowsAndUnixOptions, Constants.DefaultRegexTimeout);

    private static readonly Regex WindowsDirectories = new(
        """^([a-z]:[\\\/]?|[\\\/][\\\/][^\\\/]+[\\\/]|[\\\/])(windows[\\\/])?te?mp([\\\/]|$)""",
        WindowsAndUnixOptions,
        Constants.DefaultRegexTimeout);

    private static readonly Regex EnvironmentVariables = new($@"^%({InsecureEnvironmentVariables.JoinStr("|")})%([\\\/]|$)", WindowsAndUnixOptions, Constants.DefaultRegexTimeout);

    private readonly DiagnosticDescriptor rule;

    protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

    private protected abstract bool IsGetTempPathAssignment(TInvocationExpression invocationExpression, KnownType type, string methodName, SemanticModel model);
    private protected abstract bool IsInsecureEnvironmentVariableRetrieval(TInvocationExpression invocation, KnownType type, string methodName, SemanticModel model);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

    protected PubliclyWritableDirectoriesBase() =>
        rule = Language.CreateDescriptor(DiagnosticId, MessageFormat);

    protected override void Initialize(SonarAnalysisContext context)
    {
        var kinds = Language.SyntaxKind.StringLiteralExpressions.ToList();
        kinds.Add(Language.SyntaxKind.InterpolatedStringExpression);

        context.RegisterNodeAction(
            Language.GeneratedCodeRecognizer,
            c =>
            {
                if (Language.Syntax.StringValue(c.Node, c.Model) is { } stringValue
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
                if (c.Node is TInvocationExpression invocation
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

    private static string[] LinuxDirs() =>
        [
            "/dev/mqueue",
            "/run/lock",
            "/var/run/lock",
        ];

    private static string[] MacDirs() =>
        [
            "/var/tmp",
            "/usr/tmp",
            "/dev/shm",
            "/library/caches",
            "/users/shared",
            "/private/tmp",
            "/private/var/tmp",
        ];
}
