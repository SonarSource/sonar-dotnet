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

namespace SonarAnalyzer.SymbolicExecution.Roslyn.RuleChecks.VisualBasic;

public class LocksReleasedAllPaths : LocksReleasedAllPathsBase
{
    public static readonly DiagnosticDescriptor S2222 = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    protected override DiagnosticDescriptor Rule => S2222;

    protected override ISafeSyntaxWalker CreateSyntaxWalker(LockAcquireReleaseCollector collector) =>
        new LockAcquireReleaseWalker(collector);

    private sealed class LockAcquireReleaseWalker : SafeVisualBasicSyntaxWalker
    {
        private readonly LockAcquireReleaseCollector collector;

        public LockAcquireReleaseWalker(LockAcquireReleaseCollector collector) =>
            this.collector = collector;

        public override void Visit(SyntaxNode node)
        {
            if (collector.LockAcquiredAndReleased
                || node is LambdaExpressionSyntax)
            {
                return;
            }

            base.Visit(node);
        }

        public override void VisitIdentifierName(IdentifierNameSyntax node)
        {
            collector.RegisterIdentifier(node.Identifier.ValueText);
            base.VisitIdentifierName(node);
        }
    }
}
