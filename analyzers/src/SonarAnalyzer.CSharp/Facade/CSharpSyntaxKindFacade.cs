﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.CodeAnalysis.CSharp;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Helpers.Facade
{
    internal sealed class CSharpSyntaxKindFacade : ISyntaxKindFacade<SyntaxKind>
    {
        public SyntaxKind Attribute => SyntaxKind.Attribute;
        public SyntaxKind[] ClassAndRecordDeclaration => new[] {SyntaxKind.ClassDeclaration, SyntaxKindEx.RecordDeclaration};
        public SyntaxKind EnumDeclaration => SyntaxKind.EnumDeclaration;
        public SyntaxKind IdentifierName => SyntaxKind.IdentifierName;
        public SyntaxKind IdentifierToken => SyntaxKind.IdentifierToken;
        public SyntaxKind InvocationExpression => SyntaxKind.InvocationExpression;
        public SyntaxKind InterpolatedStringExpression => SyntaxKind.InterpolatedStringExpression;
        public SyntaxKind[] MethodDeclarations => new[] { SyntaxKind.MethodDeclaration };
        public SyntaxKind[] ObjectCreationExpressions => new[] {SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression};
        public SyntaxKind ReturnStatement => SyntaxKind.ReturnStatement;
        public SyntaxKind SimpleMemberAccessExpression => SyntaxKind.SimpleMemberAccessExpression;
        public SyntaxKind StringLiteralExpression => SyntaxKind.StringLiteralExpression;
        public SyntaxKind[] TypeDeclaration => new[]
        {
            SyntaxKind.ClassDeclaration,
            SyntaxKind.StructDeclaration,
            SyntaxKind.InterfaceDeclaration,
            SyntaxKind.EnumDeclaration,
            SyntaxKindEx.RecordDeclaration
        };
        public SyntaxKind DefaultLiteral => SyntaxKindEx.DefaultLiteralExpression;
    }
}
