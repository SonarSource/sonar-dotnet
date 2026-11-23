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

namespace SonarAnalyzer.VisualBasic.Core.Facade.Implementation;

internal sealed class VisualBasicSyntaxKindFacade : SyntaxKindFacadeBase<SyntaxKind>, ISyntaxKindFacade<SyntaxKind>
{
    public SyntaxKind Attribute => SyntaxKind.Attribute;
    public SyntaxKind AttributeArgument => SyntaxKind.SimpleArgument;
    public SyntaxKind[] CastExpressions => new[] { SyntaxKind.CTypeExpression, SyntaxKind.DirectCastExpression };
    public SyntaxKind ClassDeclaration => SyntaxKind.ClassBlock;
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
    public SyntaxKind VariableDeclarator => SyntaxKind.VariableDeclarator;
    public SyntaxKind[] LocalDeclarationKinds => [SyntaxKind.ModifiedIdentifier];
    public SyntaxKind WhitespaceTrivia => SyntaxKind.WhitespaceTrivia;

    protected override SyntaxKind Kind(SyntaxNode node) =>
        node.Kind();

    protected override SyntaxKind[] MapTypeDeclaration(TypeDeclarationKinds kind) =>
        kind switch
        {
            TypeDeclarationKinds.Class => [SyntaxKind.ClassBlock],
            TypeDeclarationKinds.Module => [SyntaxKind.ModuleBlock],
            TypeDeclarationKinds.Struct => [SyntaxKind.StructureBlock],
            TypeDeclarationKinds.Interface => [SyntaxKind.InterfaceBlock],
            TypeDeclarationKinds.Enum => [SyntaxKind.EnumBlock],
            TypeDeclarationKinds.Delegate => [SyntaxKind.DelegateSubStatement, SyntaxKind.DelegateFunctionStatement],
            TypeDeclarationKinds.Record or TypeDeclarationKinds.RecordStruct or TypeDeclarationKinds.Union or TypeDeclarationKinds.Extension => [],
            _ => throw new UnexpectedValueException(nameof(kind), kind.ToString())
        };
}
