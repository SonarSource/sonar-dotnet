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
    public class VisualBasicMethodDeclarationTracker : MethodDeclarationTracker<SyntaxKind>
    {
        protected override ILanguageFacade<SyntaxKind> Language => VisualBasicFacade.Instance;

        public override Condition ParameterAtIndexIsUsed(int index) =>
            context =>
            {
                var parameterSymbol = context.MethodSymbol.Parameters.ElementAtOrDefault(0);
                if (parameterSymbol == null)
                {
                    return false;
                }

                var methodDeclaration = context.MethodSymbol.DeclaringSyntaxReferences
                    .Select(r => (MethodBlockSyntax)r.GetSyntax().Parent)
                    .FirstOrDefault(HasImplementation);

                if (methodDeclaration == null)
                {
                    return false;
                }

                var semanticModel = context.GetSemanticModel(methodDeclaration);

                var descendantNodes = methodDeclaration.Statements
                    .SelectMany(statement => statement.DescendantNodes());

                return descendantNodes.Any(
                    node =>
                        node.IsKind(SyntaxKind.IdentifierName)
                        && ((IdentifierNameSyntax)node).Identifier.ValueText == parameterSymbol.Name
                        && parameterSymbol.Equals(semanticModel.GetSymbolInfo(node).Symbol));
            };

        protected override SyntaxToken? GetMethodIdentifier(SyntaxNode methodDeclaration) =>
            methodDeclaration switch
            {
                SubNewStatementSyntax constructor => constructor.NewKeyword,
                MethodStatementSyntax method => method.Identifier,
                OperatorStatementSyntax op => op.OperatorToken,
                _ => methodDeclaration?.Parent.Parent switch
                {
                    EventBlockSyntax e => e.EventStatement.Identifier,
                    PropertyBlockSyntax p => p.PropertyStatement.Identifier,
                    _ => null
                }
            };

        private static bool HasImplementation(MethodBlockSyntax methodBlock) =>
            methodBlock.Statements.Count > 0;
    }
}
