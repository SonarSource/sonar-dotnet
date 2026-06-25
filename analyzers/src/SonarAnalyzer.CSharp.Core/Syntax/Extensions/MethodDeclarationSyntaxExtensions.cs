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

namespace SonarAnalyzer.CSharp.Core.Syntax.Extensions;

public static class MethodDeclarationSyntaxExtensions
{
    extension(BaseMethodDeclarationSyntax methodDeclaration)
    {
        public bool IsExtensionMethod =>
            methodDeclaration.ParameterList.Parameters.Count > 0
            && methodDeclaration.ParameterList.Parameters[0].Modifiers.Any(s => s.IsKind(SyntaxKind.ThisKeyword));
    }

    extension(MethodDeclarationSyntax methodDeclaration)
    {
        /// <summary>
        /// Returns true if the method throws exceptions or returns null.
        /// </summary>
        public bool ThrowsOrReturnsNull() =>
            methodDeclaration.DescendantNodes().OfType<ThrowStatementSyntax>().Any()
            || methodDeclaration.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKindEx.ThrowExpression))
            || methodDeclaration.DescendantNodes().OfType<ReturnStatementSyntax>().Any(returnStatement => returnStatement.Expression.IsKind(SyntaxKind.NullLiteralExpression))
            // For simplicity this returns true for any method witch contains a NullLiteralExpression but this could be a source of FNs
            || methodDeclaration.DescendantNodes().OfType<ExpressionSyntax>().Any(expression => expression.IsKind(SyntaxKind.NullLiteralExpression));

        public bool HasReturnTypeVoid => methodDeclaration.ReturnType is PredefinedTypeSyntax { Keyword.RawKind: (int)SyntaxKind.VoidKeyword };

        public bool IsDeconstructor
        {
            get
            {
                return methodDeclaration.HasReturnTypeVoid
                    && (methodDeclaration.IsExtensionMethod || !methodDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                    && methodDeclaration.Identifier.Value.Equals("Deconstruct")
                    && AllParametersHaveModifierOut(methodDeclaration);

                static bool AllParametersHaveModifierOut(MethodDeclarationSyntax methodDeclaration) =>
                    (methodDeclaration.IsExtensionMethod
                         ? methodDeclaration.ParameterList.Parameters.Skip(1)
                         : methodDeclaration.ParameterList.Parameters)
                    .All(x => x.Modifiers.Any(y => y.IsKind(SyntaxKind.OutKeyword)));
            }
        }
    }
}
