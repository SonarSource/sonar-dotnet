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
using System.Resources;
using System.Text.RegularExpressions;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules
{
    public abstract class PubliclyWritableDirectoriesBase<TSyntaxKind, TInvocationExpression> : HotspotDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TInvocationExpression : SyntaxNode
    {
        protected const string DiagnosticId = "S5443";
        private const string MessageFormat = "Make sure publicly writable directories are used safely here.";
        private const RegexOptions WindowsAndUnixOptions = RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant;
        private const string ForwardSlashTwoPartTemplate = @"\/{0}\/{1}";
        private const string ForwardSlashThreePartTemplate = @"\/{0}\/{1}\/{2}";
        private readonly Regex userProfile = new Regex(@"^%USERPROFILE%[\\\/]AppData[\\\/]Local[\\\/]Temp", WindowsAndUnixOptions);
        private readonly Regex linuxDirectories;
        private readonly Regex macDirectories;
        private readonly Regex windowsDirectories;
        private readonly Regex environmentVariables;
        private readonly DiagnosticDescriptor rule;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        private protected abstract bool IsGetTempPathAssignment(TInvocationExpression invocationExpression, KnownType type, string methodName, SemanticModel semanticModel);
        private protected abstract bool IsInsecureEnvironmentVariableRetrieval(TInvocationExpression invocation, KnownType type, string methodName, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected string[] InsecureEnvironmentVariables { get; } = { "tmp", "temp", "tmpdir" };

        protected PubliclyWritableDirectoriesBase(IAnalyzerConfiguration configuration, ResourceManager rspecResources) : base(configuration)
        {
            var varTmp = string.Format(ForwardSlashTwoPartTemplate, "var", "tmp");
            var usrTmp = string.Format(ForwardSlashTwoPartTemplate, "usr", "tmp");
            var devShm = string.Format(ForwardSlashTwoPartTemplate, "dev", "shm");
            var devMqueue = string.Format(ForwardSlashTwoPartTemplate, "dev", "mqueue");
            var runLock = string.Format(ForwardSlashTwoPartTemplate, "run", "lock");
            var varRunLock = string.Format(ForwardSlashThreePartTemplate, "var", "run", "lock");
            var libraryCaches = string.Format(ForwardSlashTwoPartTemplate, "library", "caches");
            var usersShared = string.Format(ForwardSlashTwoPartTemplate, "users", "shared");
            var privateTmp = string.Format(ForwardSlashTwoPartTemplate, "private", "tmp");
            var privateVarTmp = string.Format(ForwardSlashThreePartTemplate, "private", "var", "tmp");
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();
            linuxDirectories = new Regex($@"^({devMqueue}|{runLock}|{varRunLock})(\/|$)", RegexOptions.Compiled);
            macDirectories = new Regex($@"^({libraryCaches}|{usersShared}|{privateTmp}|{privateVarTmp}|{varTmp}|{usrTmp}|{devShm})(\/|$)", WindowsAndUnixOptions);
            windowsDirectories = new Regex(@"^([a-z]:[\\\/]?|[\\\/][\\\/][^\\\/]+[\\\/]|[\\\/])(windows[\\\/])?te?mp([\\\/]|$)", WindowsAndUnixOptions);
            environmentVariables = new Regex($@"^%({InsecureEnvironmentVariables.JoinStr("|")})%([\\\/]|$)", WindowsAndUnixOptions);
        }

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (IsEnabled(c.Options)
                        && Language.Syntax.NodeStringTextValue(c.Node) is { } stringValue
                        && IsSensitiveDirectoryUsage(stringValue))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
                    }
                },
                Language.SyntaxKind.StringLiteralExpression,
                Language.SyntaxKind.InterpolatedStringExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (IsEnabled(c.Options)
                        && c.Node is TInvocationExpression invocation
                        && (IsGetTempPathAssignment(invocation, KnownType.System_IO_Path, "GetTempPath", c.SemanticModel)
                            || IsInsecureEnvironmentVariableRetrieval(invocation, KnownType.System_Environment, "GetEnvironmentVariable", c.SemanticModel)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, c.Node.GetLocation()));
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
