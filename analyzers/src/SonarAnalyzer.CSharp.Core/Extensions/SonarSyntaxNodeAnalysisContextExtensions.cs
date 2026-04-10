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

namespace SonarAnalyzer.CSharp.Core.Extensions;

public static class SonarSyntaxNodeAnalysisContextExtensions
{
    extension(SonarSyntaxNodeReportingContext context)
    {
        public bool IsTopLevelMain =>
            context.Node is CompilationUnitSyntax compilationUnitSyntax
            && compilationUnitSyntax.IsTopLevelMain()
            && context.ContainingSymbol.IsGlobalNamespace(); // Needed to avoid the duplicate calls from Roslyn 4.0.0

        public bool IsInExpressionTree() =>
            context.Node.IsInExpressionTree(context.Model);
    }
}
