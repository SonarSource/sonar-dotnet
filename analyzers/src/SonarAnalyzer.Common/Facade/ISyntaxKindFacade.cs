/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2022 SonarSource SA
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

namespace SonarAnalyzer.Helpers.Facade;

public interface ISyntaxKindFacade<out TSyntaxKind>
    where TSyntaxKind : struct
{
    abstract TSyntaxKind Attribute { get; }
    abstract TSyntaxKind[] ClassAndRecordDeclaration { get; }
    abstract TSyntaxKind ClassDeclaration { get; }
    abstract TSyntaxKind[] ComparisonKinds { get; }
    abstract TSyntaxKind ConstructorDeclaration { get; }
    abstract TSyntaxKind[] DefaultExpressions { get; }
    abstract TSyntaxKind EnumDeclaration { get; }
    abstract TSyntaxKind FieldDeclaration { get; }
    abstract TSyntaxKind IdentifierName { get; }
    abstract TSyntaxKind IdentifierToken { get; }
    abstract TSyntaxKind InvocationExpression { get; }
    abstract TSyntaxKind InterpolatedStringExpression { get; }
    abstract TSyntaxKind[] MethodDeclarations { get; }
    abstract TSyntaxKind[] ObjectCreationExpressions { get; }
    abstract TSyntaxKind Parameter { get; }
    abstract TSyntaxKind ParameterList { get; }
    abstract TSyntaxKind ReturnStatement { get; }
    abstract TSyntaxKind SimpleAssignment { get; }
    abstract TSyntaxKind SimpleMemberAccessExpression { get; }
    abstract TSyntaxKind StringLiteralExpression { get; }
    abstract TSyntaxKind[] TypeDeclaration { get; }
}
