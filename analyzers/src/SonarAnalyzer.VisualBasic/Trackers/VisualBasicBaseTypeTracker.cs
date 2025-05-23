﻿/*
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

namespace SonarAnalyzer.Helpers.Trackers
{
    public class VisualBasicBaseTypeTracker : BaseTypeTracker<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;
        protected override SyntaxKind[] TrackedSyntaxKinds { get; } = new[] { SyntaxKind.InheritsStatement, SyntaxKind.ImplementsStatement };

        protected override IEnumerable<SyntaxNode> GetBaseTypeNodes(SyntaxNode contextNode) =>
            // VB has separate Inherits and Implements keywords so the base types
            // are in separate lists under different types of syntax node.
            // If a class both inherits and implements then this tracker will check
            // the conditions against Inherits and Implements *separately*
            // i.e. the conditions will be called twice
            contextNode switch
            {
                InheritsStatementSyntax inherits => inherits.Types,
                ImplementsStatementSyntax implements => implements.Types,
                _ => Enumerable.Empty<SyntaxNode>(),
            };
    }
}
