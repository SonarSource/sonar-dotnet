/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

namespace SonarAnalyzer.CSharp.Core.Facade.Implementation;

internal sealed class CSharpSyntaxKindFacade : ISyntaxKindFacade<SyntaxKind>
{
    public SyntaxKind Attribute => SyntaxKind.Attribute;
    public SyntaxKind AttributeArgument => SyntaxKind.AttributeArgument;
    public SyntaxKind[] CastExpressions => new[] {SyntaxKind.CastExpression };
    public SyntaxKind ClassDeclaration => SyntaxKind.ClassDeclaration;
    public SyntaxKind[] ClassAndRecordDeclarations => new[]
    {
        SyntaxKind.ClassDeclaration,
        SyntaxKindEx.RecordDeclaration,
    };
    public SyntaxKind[] ClassAndModuleDeclarations => new[] { SyntaxKind.ClassDeclaration };
    public SyntaxKind[] ComparisonKinds => new[]
    {
        SyntaxKind.GreaterThanExpression,
        SyntaxKind.GreaterThanOrEqualExpression,
        SyntaxKind.LessThanExpression,
        SyntaxKind.LessThanOrEqualExpression,
        SyntaxKind.EqualsExpression,
        SyntaxKind.NotEqualsExpression,
    };
    public SyntaxKind ConstructorDeclaration => SyntaxKind.ConstructorDeclaration;
    public SyntaxKind[] DefaultExpressions => new[] { SyntaxKind.DefaultExpression, SyntaxKindEx.DefaultLiteralExpression };
    public SyntaxKind EnumDeclaration => SyntaxKind.EnumDeclaration;
    public SyntaxKind EndOfLineTrivia => SyntaxKind.EndOfLineTrivia;
    public SyntaxKind FieldDeclaration => SyntaxKind.FieldDeclaration;
    public SyntaxKind IdentifierName => SyntaxKind.IdentifierName;
    public SyntaxKind IdentifierToken => SyntaxKind.IdentifierToken;
    public SyntaxKind InvocationExpression => SyntaxKind.InvocationExpression;
    public SyntaxKind InterpolatedStringExpression => SyntaxKind.InterpolatedStringExpression;
    public SyntaxKind LeftShiftAssignmentStatement => SyntaxKind.LeftShiftAssignmentExpression;
    public SyntaxKind LeftShiftExpression => SyntaxKind.LeftShiftExpression;
    public SyntaxKind LocalDeclaration => SyntaxKind.LocalDeclarationStatement;
    public SyntaxKind[] MethodDeclarations => new[] { SyntaxKind.MethodDeclaration };
    public SyntaxKind[] ObjectCreationExpressions => new[] { SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression };
    public SyntaxKind Parameter => SyntaxKind.Parameter;
    public SyntaxKind ParameterList => SyntaxKind.ParameterList;
    public SyntaxKind RefKeyword => SyntaxKind.RefKeyword;
    public SyntaxKind ReturnStatement => SyntaxKind.ReturnStatement;
    public SyntaxKind RightShiftAssignmentStatement => SyntaxKind.RightShiftAssignmentExpression;
    public SyntaxKind RightShiftExpression => SyntaxKind.RightShiftExpression;
    public SyntaxKind SimpleAssignment => SyntaxKind.SimpleAssignmentExpression;
    public SyntaxKind SimpleCommentTrivia => SyntaxKind.SingleLineCommentTrivia;
    public SyntaxKind SimpleMemberAccessExpression => SyntaxKind.SimpleMemberAccessExpression;
    public SyntaxKind[] StringLiteralExpressions => new[] { SyntaxKind.StringLiteralExpression, SyntaxKindEx.Utf8StringLiteralExpression };
    public SyntaxKind StructDeclaration => SyntaxKind.StructDeclaration;
    public SyntaxKind SubtractExpression => SyntaxKind.SubtractExpression;
    public SyntaxKind[] TypeDeclaration => new[]
    {
        SyntaxKind.ClassDeclaration,
        SyntaxKind.StructDeclaration,
        SyntaxKind.InterfaceDeclaration,
        SyntaxKind.EnumDeclaration,
        SyntaxKindEx.RecordDeclaration,
        SyntaxKindEx.RecordStructDeclaration,
    };
    public SyntaxKind VariableDeclarator => SyntaxKind.VariableDeclarator;
    public SyntaxKind WhitespaceTrivia => SyntaxKind.WhitespaceTrivia;
}
