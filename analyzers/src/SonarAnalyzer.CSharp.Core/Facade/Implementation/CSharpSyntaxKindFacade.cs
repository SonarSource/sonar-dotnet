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

namespace SonarAnalyzer.CSharp.Core.Facade.Implementation;

internal sealed class CSharpSyntaxKindFacade : SyntaxKindFacadeBase<SyntaxKind>, ISyntaxKindFacade<SyntaxKind>
{
    public static CSharpSyntaxKindFacade Instance { get; } = new();

    public SyntaxKind Attribute => SyntaxKind.Attribute;
    public SyntaxKind AttributeArgument => SyntaxKind.AttributeArgument;
    public SyntaxKind[] CastExpressions => new[] {SyntaxKind.CastExpression };
    public SyntaxKind ClassDeclaration => SyntaxKind.ClassDeclaration;
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
    public SyntaxKind VariableDeclarator => SyntaxKind.VariableDeclarator;

    public SyntaxKind[] LocalDeclarationKinds =>
    [
        SyntaxKind.VariableDeclarator,
        SyntaxKindEx.SingleVariableDesignation, // x is T y, out var x, var (a,b)=..., patterns
        SyntaxKind.CatchDeclaration,            // catch (Exception e)
        SyntaxKind.ForEachStatement,            // foreach (var x in ...)
        SyntaxKind.FromClause,                  // from x in ...
        SyntaxKind.JoinClause,                  // join x in coll on a equals b
        SyntaxKind.JoinIntoClause,              // join ... into x
        SyntaxKind.LetClause,                   // let x = ...
        SyntaxKind.QueryContinuation,           // ... into x select
    ];
    public SyntaxKind WhitespaceTrivia => SyntaxKind.WhitespaceTrivia;

    protected override SyntaxKind Kind(SyntaxNode node) =>
        node.Kind();

    protected override SyntaxKind[] MapTypeDeclaration(TypeDeclarationKinds kind) =>
        kind switch
        {
            TypeDeclarationKinds.Class => [SyntaxKind.ClassDeclaration],
            TypeDeclarationKinds.Struct => [SyntaxKind.StructDeclaration],
            TypeDeclarationKinds.Record => [SyntaxKindEx.RecordDeclaration],
            TypeDeclarationKinds.RecordStruct => [SyntaxKindEx.RecordStructDeclaration],
            TypeDeclarationKinds.Union => [SyntaxKindEx.UnionDeclaration],
            TypeDeclarationKinds.Interface => [SyntaxKind.InterfaceDeclaration],
            TypeDeclarationKinds.Enum => [SyntaxKind.EnumDeclaration],
            TypeDeclarationKinds.Delegate => [SyntaxKind.DelegateDeclaration],
            TypeDeclarationKinds.Extension => [SyntaxKindEx.ExtensionBlockDeclaration],
            TypeDeclarationKinds.Module => [],
            _ => throw new UnexpectedValueException(nameof(kind), kind.ToString())
        };
}
