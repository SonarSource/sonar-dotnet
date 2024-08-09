/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

namespace SonarAnalyzer.Rules
{
    public abstract class ExpectedExceptionAttributeShouldNotBeUsedBase<TSyntaxKind> : SonarDiagnosticAnalyzer<TSyntaxKind>
        where TSyntaxKind : struct
    {
        internal const string DiagnosticId = "S3431";

        protected abstract SyntaxNode FindExpectedExceptionAttribute(SyntaxNode node);
        protected abstract bool HasMultiLineBody(SyntaxNode node);
        protected abstract bool AssertInCatchFinallyBlock(SyntaxNode node);

        protected override string MessageFormat => "Replace the 'ExpectedException' attribute with a throw assertion or a try/catch block.";

        protected ExpectedExceptionAttributeShouldNotBeUsedBase() : base(DiagnosticId) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(c =>
            {
                if (!ContainExpectedExceptionType(c.Compilation))
                {
                    return;
                }

                c.RegisterNodeAction(Language.GeneratedCodeRecognizer, cc =>
                    {
                        if (FindExpectedExceptionAttribute(cc.Node) is {} attribute
                            && HasMultiLineBody(cc.Node)
                            && !AssertInCatchFinallyBlock(cc.Node))
                        {
                            cc.ReportIssue(Rule, attribute);
                        }
                    },
                    Language.SyntaxKind.MethodDeclarations);
            });

        private static bool ContainExpectedExceptionType(Compilation compilation) =>
            compilation.GetTypeByMetadataName(KnownType.Microsoft_VisualStudio_TestTools_UnitTesting_ExpectedExceptionAttribute) is not null
            || compilation.GetTypeByMetadataName(KnownType.NUnit_Framework_ExpectedExceptionAttribute) is not null;
    }
}
