/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2023 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class PropertyToAutoProperty : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2292";
        private const string MessageFormat = "Make this an auto-implemented property and remove its backing field.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(
                c =>
                {
                    const int Count = 2;
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
                    if (propertyDeclaration.AccessorList == null
                        || propertyDeclaration.AccessorList.Accessors.Count != Count
                        || propertySymbol == null
                        || HasDifferentModifiers(propertyDeclaration.AccessorList.Accessors)
                        || HasAttributes(propertyDeclaration.AccessorList.Accessors))
                    {
                        return;
                    }

                    var accessors = propertyDeclaration.AccessorList.Accessors;
                    var getter = accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                    var setter = accessors.FirstOrDefault(a => a.IsAnyKind(SyntaxKind.SetAccessorDeclaration, SyntaxKindEx.InitAccessorDeclaration));

                    if (getter != null
                        && setter != null
                        && FieldFromGetter(getter, c.SemanticModel) is { } getterField
                        && FieldFromSetter(setter, c.SemanticModel) is { } setterField
                        && getterField.Equals(setterField)
                        && !getterField.GetAttributes().Any()
                        && !getterField.IsVolatile
                        && getterField.IsStatic == propertySymbol.IsStatic
                        && getterField.Type.Equals(propertySymbol.Type))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, propertyDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.PropertyDeclaration);

        private static bool HasAttributes(SyntaxList<AccessorDeclarationSyntax> accessors) =>
            accessors.Any(a => a.AttributeLists.Any());

        private static bool HasDifferentModifiers(SyntaxList<AccessorDeclarationSyntax> accessors)
        {
            var modifiers = ModifierKinds(accessors.First()).ToHashSet();
            return accessors.Skip(1).Any(a => !modifiers.SetEquals(ModifierKinds(a)));
        }

        private static IEnumerable<SyntaxKind> ModifierKinds(AccessorDeclarationSyntax accessor) =>
            accessor.Modifiers.Select(m => m.Kind());

        private static IFieldSymbol FieldFromSetter(AccessorDeclarationSyntax setter, SemanticModel semanticModel)
        {
            var assignment = AssignmentFromBody(setter.Body) ?? AssignmentFromExpressionBody(setter.ExpressionBody());

            return assignment != null
                   && assignment.IsKind(SyntaxKind.SimpleAssignmentExpression)
                   && assignment.Right != null
                   && semanticModel.GetSymbolInfo(assignment.Right).Symbol is IParameterSymbol {Name: "value", IsImplicitlyDeclared: true}
                   ? FieldSymbol(assignment.Left, semanticModel.GetDeclaredSymbol(setter).ContainingType, semanticModel)
                   : null;

            AssignmentExpressionSyntax AssignmentFromBody(BlockSyntax body) =>
                body?.Statements.Count == 1 && body.Statements[0] is ExpressionStatementSyntax statement
                ? statement.Expression as AssignmentExpressionSyntax
                : null;

            AssignmentExpressionSyntax AssignmentFromExpressionBody(ArrowExpressionClauseSyntax expressionBody) =>
                expressionBody?.ChildNodes().Count() == 1
                ? expressionBody.ChildNodes().Single() as AssignmentExpressionSyntax
                : null;
        }

        private static IFieldSymbol FieldSymbol(ExpressionSyntax expression, INamedTypeSymbol declaringType, SemanticModel semanticModel)
        {
            if (expression is IdentifierNameSyntax)
            {
                return semanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol;
            }
            else if (expression is MemberAccessExpressionSyntax memberAccess && memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                return memberAccess.Expression is ThisExpressionSyntax
                       || (memberAccess.Expression is IdentifierNameSyntax identifier && semanticModel.GetSymbolInfo(identifier).Symbol is INamedTypeSymbol type && type.Equals(declaringType))
                       ? semanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol
                       : null;
            }
            else
            {
                return null;
            }
        }

        private static IFieldSymbol FieldFromGetter(AccessorDeclarationSyntax getter, SemanticModel semanticModel)
        {
            var returnedExpression = GetReturnExpressionFromBody(getter.Body) ?? getter.ExpressionBody()?.Expression;

            return returnedExpression == null
                   ? null
                   : FieldSymbol(returnedExpression, semanticModel.GetDeclaredSymbol(getter).ContainingType, semanticModel);

            ExpressionSyntax GetReturnExpressionFromBody(BlockSyntax body) =>
                body != null && body.Statements.Count == 1 && body.Statements[0] is ReturnStatementSyntax returnStatement
                ? returnStatement.Expression
                : null;
        }
    }
}
