/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2019 SonarSource SA
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

using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers
{
    public class VisualBasicBaseTypeTracker : BaseTypeTracker<SyntaxKind>
    {
        public VisualBasicBaseTypeTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule)
            : base(analyzerConfiguration, rule)
        {
        }

        protected override SyntaxKind[] TrackedSyntaxKinds { get; } =
            new[] { SyntaxKind.InheritsStatement, SyntaxKind.ImplementsStatement };

        protected override GeneratedCodeRecognizer GeneratedCodeRecognizer { get; } =
            VisualBasic.VisualBasicGeneratedCodeRecognizer.Instance;

        protected override IEnumerable<SyntaxNode> GetBaseTypeNodes(SyntaxNode contextNode)
        {
            // VB has separate Inherits and Implements keywords so the base types
            // are in separate lists under different types of syntax node.
            // If a class both inherits and implements then this tracker will check
            // the conditions against Inherits and Implements *separately*
            // i.e. the conditions will be called twice
            switch (contextNode.RawKind)
            {
                case (int)SyntaxKind.InheritsStatement:
                    return ((InheritsStatementSyntax)contextNode).Types;
                case (int)SyntaxKind.ImplementsStatement:
                    return ((ImplementsStatementSyntax)contextNode).Types;
                default:
                    return Enumerable.Empty<SyntaxNode>();
            }
        }
    }
}

