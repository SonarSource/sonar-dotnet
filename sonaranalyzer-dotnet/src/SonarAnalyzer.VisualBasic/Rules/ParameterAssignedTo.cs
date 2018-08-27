/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.VisualBasic;
using Microsoft.CodeAnalysis.VisualBasic.Syntax;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.Rules.VisualBasic
{
    [DiagnosticAnalyzer(LanguageNames.VisualBasic)]
    [Rule(DiagnosticId)]
    public sealed class ParameterAssignedTo : ParameterAssignedToBase<SyntaxKind, AssignmentStatementSyntax>
    {
        private static readonly DiagnosticDescriptor rule =
           DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ImmutableArray<SyntaxKind> kindsOfInterest = ImmutableArray.Create(
            SyntaxKind.AddAssignmentStatement,
            SyntaxKind.SimpleAssignmentStatement,
            SyntaxKind.SubtractAssignmentStatement,
            SyntaxKind.MultiplyAssignmentStatement,
            SyntaxKind.DivideAssignmentStatement,
            SyntaxKind.MidAssignmentStatement,
            SyntaxKind.ConcatenateAssignmentStatement,
            SyntaxKind.ExponentiateAssignmentStatement,
            SyntaxKind.IntegerDivideAssignmentStatement,
            SyntaxKind.LeftShiftAssignmentStatement,
            SyntaxKind.RightShiftAssignmentStatement
            );

        public override ImmutableArray<SyntaxKind> SyntaxKindsOfInterest => kindsOfInterest;

        protected override bool IsAssignmentToCatchVariable(ISymbol symbol, SyntaxNode node)
        {
            var localSymbol = symbol as ILocalSymbol;

            // this could mimic the C# variant too, but that doesn't work:
            // https://github.com/dotnet/roslyn/issues/6209
            // so:
            var location = localSymbol?.Locations.FirstOrDefault();
            if (location == null)
            {
                return false;
            }

            if (!(node.SyntaxTree.GetRoot().FindNode(location.SourceSpan, getInnermostNodeForTie: true) is IdentifierNameSyntax declarationName))
            {
                return false;
            }

            return declarationName.Parent is CatchStatementSyntax catchStatement
                && catchStatement.IdentifierName == declarationName;
        }

        protected override bool IsAssignmentToParameter(ISymbol symbol)
        {
            var parameterSymbol = symbol as IParameterSymbol;

            return parameterSymbol?.RefKind == RefKind.None;
        }

        protected override SyntaxNode GetAssignedNode(AssignmentStatementSyntax assignment) => assignment.Left;

        protected override bool IsReadBefore(SemanticModel semanticModel, ISymbol parameterSymbol, AssignmentStatementSyntax assignment)
        {
            // Implement logic https://jira.sonarsource.com/browse/SONARVB-236
            return false;
        }

        protected sealed override GeneratedCodeRecognizer GeneratedCodeRecognizer => Helpers.VisualBasic.GeneratedCodeRecognizer.Instance;
    }
}
