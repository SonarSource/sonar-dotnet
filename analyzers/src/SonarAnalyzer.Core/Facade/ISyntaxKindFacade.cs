/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Facade;

public interface ISyntaxKindFacade<out TSyntaxKind> where TSyntaxKind : struct
{
    abstract TSyntaxKind Attribute { get; }
    abstract TSyntaxKind AttributeArgument { get; }
    abstract TSyntaxKind[] CastExpressions { get; }
    abstract TSyntaxKind ClassDeclaration { get; }
    abstract TSyntaxKind[] ClassAndRecordDeclarations { get; }
    abstract TSyntaxKind[] ClassAndModuleDeclarations { get; }
    abstract TSyntaxKind[] ComparisonKinds { get; }
    abstract TSyntaxKind ConstructorDeclaration { get; }
    abstract TSyntaxKind[] DefaultExpressions { get; }
    abstract TSyntaxKind EndOfLineTrivia { get; }
    abstract TSyntaxKind EnumDeclaration { get; }
    abstract TSyntaxKind FieldDeclaration { get; }
    abstract TSyntaxKind IdentifierName { get; }
    abstract TSyntaxKind IdentifierToken { get; }
    abstract TSyntaxKind InvocationExpression { get; }
    abstract TSyntaxKind InterpolatedStringExpression { get; }
    abstract TSyntaxKind LeftShiftAssignmentStatement { get; }
    abstract TSyntaxKind LeftShiftExpression { get; }
    abstract TSyntaxKind LocalDeclaration { get; }
    abstract TSyntaxKind[] MethodDeclarations { get; }
    abstract TSyntaxKind[] ObjectCreationExpressions { get; }
    abstract TSyntaxKind Parameter { get; }
    abstract TSyntaxKind RefKeyword { get; }
    abstract TSyntaxKind RightShiftExpression { get; }
    abstract TSyntaxKind RightShiftAssignmentStatement { get; }
    abstract TSyntaxKind ParameterList { get; }
    abstract TSyntaxKind ReturnStatement { get; }
    abstract TSyntaxKind SimpleAssignment { get; }
    abstract TSyntaxKind SimpleCommentTrivia { get; }
    abstract TSyntaxKind SimpleMemberAccessExpression { get; }
    abstract TSyntaxKind[] StringLiteralExpressions { get; }
    abstract TSyntaxKind StructDeclaration { get; }
    abstract TSyntaxKind SubtractExpression { get; }
    abstract TSyntaxKind[] TypeDeclaration { get; }
    abstract TSyntaxKind VariableDeclarator { get; }
    abstract TSyntaxKind WhitespaceTrivia { get; }
}
