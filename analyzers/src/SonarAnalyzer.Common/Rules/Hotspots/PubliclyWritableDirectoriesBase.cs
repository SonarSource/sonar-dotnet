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
        private readonly DiagnosticDescriptor rule;
        private static readonly PubliclyWritableDirectoriesRegexMatcher matcher = new PubliclyWritableDirectoriesRegexMatcher();

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        private protected abstract bool IsGetTempPathAssignment(TInvocationExpression invocationExpression, KnownType type, string methodName, SemanticModel semanticModel);
        private protected abstract bool IsInsecureEnvironmentVariableRetrieval(TInvocationExpression invocation, KnownType type, string methodName, SemanticModel semanticModel);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected string[] InsecureEnvironmentVariables { get; } = { "tmp", "temp", "tmpdir" };

        protected PubliclyWritableDirectoriesBase(IAnalyzerConfiguration configuration, ResourceManager rspecResources) : base(configuration) =>
            rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, rspecResources).WithNotConfigurable();

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (!IsEnabled(c.Options))
                    {
                        return;
                    }

                    var node = c.Node;
                    if (Language.Syntax.NodeStringTextValue(node) is { } stringValue
                        && matcher.IsSensitiveDirectoryUsage(stringValue))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation()));
                    }
                },
                Language.SyntaxKind.StringLiteralExpression,
                Language.SyntaxKind.InterpolatedStringExpression);

            context.RegisterSyntaxNodeActionInNonGenerated(
                Language.GeneratedCodeRecognizer,
                c =>
                {
                    if (!IsEnabled(c.Options))
                    {
                        return;
                    }

                    var node = c.Node;
                    if (node is TInvocationExpression invocation
                        && (IsGetTempPathAssignment(invocation, KnownType.System_IO_Path, "GetTempPath", c.SemanticModel)
                            || IsInsecureEnvironmentVariableRetrieval(invocation, KnownType.System_Environment, "GetEnvironmentVariable", c.SemanticModel)))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, node.GetLocation()));
                    }
                },
                Language.SyntaxKind.InvocationExpression);
        }

        private class PubliclyWritableDirectoriesRegexMatcher
        {
            private const string TwoPartPatternWithVariableSlash = @"^[\/\\]{0}[\/\\]{1}([\/\\]|$)";
            private const string TwoPartPatternWithForwardSlash = @"^\/{0}\/{1}(\/|$)";
            private const string ThreePartPatternWithForwardSlash = @"^\/{0}\/{1}\/{2}(\/|$)";
            private const string TmpRegex = @"^([a-zA-z]:)?[\\\/]?[\/\\]tmp([\/\\]|$)";
            private const string TempRegex = @"^([a-zA-z]:)?[\\\/]?[\/\\]temp([\/\\]|$)";
            private const string WindowsTempRegex = @"^([a-zA-z]:)?[\\\/]?[\\\/]windows[\\\/]temp([\\\/]|$)";
            private const string UserProfileRegex = @"%USERPROFILE%\\AppData\\Local\\Temp([\\\/]|$)";
            private const string EnvironmentVariableTemplate = @"%{0}%([\\\/]|$)";
            private static readonly string VarTmpRegex = string.Format(TwoPartPatternWithVariableSlash, "var", "tmp");
            private static readonly string UsrTmpRegex = string.Format(TwoPartPatternWithVariableSlash, "usr", "tmp");
            private static readonly string DevShmRegex = string.Format(TwoPartPatternWithVariableSlash, "dev", "shm");
            private static readonly string DevMqueueRegex = string.Format(TwoPartPatternWithForwardSlash, "dev", "mqueue");
            private static readonly string RunLockRegex = string.Format(TwoPartPatternWithForwardSlash, "run", "lock");
            private static readonly string VarRunLockRegex = string.Format(ThreePartPatternWithForwardSlash, "var", "run", "lock");
            private static readonly string LibraryCachesRegex = string.Format(TwoPartPatternWithForwardSlash, "library", "caches");
            private static readonly string UsersSharedRegex = string.Format(TwoPartPatternWithForwardSlash, "users", "shared");
            private static readonly string PrivateTmpRegex = string.Format(TwoPartPatternWithForwardSlash, "private", "tmp");
            private static readonly string PrivateVarTmpRegex = string.Format(ThreePartPatternWithForwardSlash, "private", "var", "tmp");
            private static readonly string TempEnvVariable = string.Format(EnvironmentVariableTemplate, "temp");
            private static readonly string TmpEnvVariable = string.Format(EnvironmentVariableTemplate, "tmp");
            private static readonly string TmpDirEnvVariable = string.Format(EnvironmentVariableTemplate, "tmpdir");

            private static readonly Regex LinuxPubliclyWritabelDirs =
                new Regex($"{DevMqueueRegex}|{RunLockRegex}|{VarRunLockRegex}",
                    RegexOptions.Compiled);

            private static readonly Regex WindowsAndMacPubliclyWritabelDirs =
                new Regex($"{TmpRegex}|{VarTmpRegex}|{UsrTmpRegex}|{DevShmRegex}|{LibraryCachesRegex}|{UsersSharedRegex}|{PrivateTmpRegex}|{PrivateVarTmpRegex}|{TempRegex}|{WindowsTempRegex}",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            private static readonly Regex EnvironmentVariables =
                new Regex($"{UserProfileRegex}|{TempEnvVariable}|{TmpEnvVariable}|{TmpDirEnvVariable}",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            public bool IsSensitiveDirectoryUsage(string directory) =>
                WindowsAndMacPubliclyWritabelDirs.IsMatch(directory)
                || LinuxPubliclyWritabelDirs.IsMatch(directory)
                || EnvironmentVariables.IsMatch(directory);
        }
    }
}
