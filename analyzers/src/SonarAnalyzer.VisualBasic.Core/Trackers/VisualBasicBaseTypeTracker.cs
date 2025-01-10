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

using SonarAnalyzer.Core.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Trackers;

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
