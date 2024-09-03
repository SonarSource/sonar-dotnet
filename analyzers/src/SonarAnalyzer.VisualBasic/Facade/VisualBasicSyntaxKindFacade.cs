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

using SonarAnalyzer.Helpers.Facade;
namespace SonarAnalyzer.VisualBasic.Core.Facade.Implementation;

internal sealed class VisualBasicSyntaxKindFacade : ISyntaxKindFacade<SyntaxKind>
{
    public SyntaxKind Attribute => SyntaxKind.Attribute;
    public SyntaxKind[] CastExpressions => new[] { SyntaxKind.CTypeExpression, SyntaxKind.DirectCastExpression };
    public SyntaxKind ClassDeclaration => SyntaxKind.ClassBlock;
    public SyntaxKind[] ClassAndRecordDeclarations => new[] { SyntaxKind.ClassBlock };
    public SyntaxKind[] ClassAndModuleDeclarations => new[]
    {
        SyntaxKind.ClassBlock,
        SyntaxKind.ModuleBlock
    };
    public SyntaxKind[] CommentTrivia => new[]
    {
        SyntaxKind.CommentTrivia,
        SyntaxKind.DocumentationCommentTrivia,
    };
    public SyntaxKind[] ComparisonKinds => new[]
    {
        SyntaxKind.GreaterThanExpression,
        SyntaxKind.GreaterThanOrEqualExpression,
        SyntaxKind.LessThanExpression,
        SyntaxKind.LessThanOrEqualExpression,
        SyntaxKind.EqualsExpression,
        SyntaxKind.NotEqualsExpression,
    };
    public SyntaxKind ConstructorDeclaration => SyntaxKind.ConstructorBlock;
    public SyntaxKind[] DefaultExpressions => new[] { SyntaxKind.NothingLiteralExpression };
    public SyntaxKind EndOfLineTrivia => SyntaxKind.EndOfLineTrivia;
    public SyntaxKind EnumDeclaration => SyntaxKind.EnumStatement;
    public SyntaxKind FieldDeclaration => SyntaxKind.FieldDeclaration;
    public SyntaxKind IdentifierName => SyntaxKind.IdentifierName;
    public SyntaxKind IdentifierToken => SyntaxKind.IdentifierToken;
    public SyntaxKind InvocationExpression => SyntaxKind.InvocationExpression;
    public SyntaxKind InterpolatedStringExpression => SyntaxKind.InterpolatedStringExpression;
    public SyntaxKind LeftShiftAssignmentStatement => SyntaxKind.LeftShiftAssignmentStatement;
    public SyntaxKind LeftShiftExpression => SyntaxKind.LeftShiftExpression;
    public SyntaxKind LocalDeclaration => SyntaxKind.LocalDeclarationStatement;
    public SyntaxKind[] MethodDeclarations => new[] { SyntaxKind.FunctionStatement, SyntaxKind.SubStatement };
    public SyntaxKind[] ObjectCreationExpressions => new[] { SyntaxKind.ObjectCreationExpression };
    public SyntaxKind Parameter => SyntaxKind.Parameter;
    public SyntaxKind ParameterList => SyntaxKind.ParameterList;
    public SyntaxKind RefKeyword => SyntaxKind.ByRefKeyword;
    public SyntaxKind ReturnStatement => SyntaxKind.ReturnStatement;
    public SyntaxKind RightShiftAssignmentStatement => SyntaxKind.RightShiftAssignmentStatement;
    public SyntaxKind RightShiftExpression => SyntaxKind.RightShiftExpression;
    public SyntaxKind SimpleAssignment => SyntaxKind.SimpleAssignmentStatement;
    public SyntaxKind SimpleCommentTrivia => SyntaxKind.CommentTrivia;
    public SyntaxKind SimpleMemberAccessExpression => SyntaxKind.SimpleMemberAccessExpression;
    public SyntaxKind[] StringLiteralExpressions => new[] { SyntaxKind.StringLiteralExpression };
    public SyntaxKind StructDeclaration => SyntaxKind.StructureBlock;
    public SyntaxKind SubtractExpression => SyntaxKind.SubtractExpression;
    public SyntaxKind[] TypeDeclaration => new[] { SyntaxKind.ClassBlock, SyntaxKind.StructureBlock, SyntaxKind.InterfaceBlock, SyntaxKind.EnumBlock };
    public SyntaxKind VariableDeclarator => SyntaxKind.VariableDeclarator;
    public SyntaxKind WhitespaceTrivia => SyntaxKind.WhitespaceTrivia;
}
