/*
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

using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using SonarAnalyzer.Common;
using SonarAnalyzer.Helpers;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class PropertyToAutoProperty : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2292";
        private const string MessageFormat = "Make this an auto-implemented property and remove its backing field.";

        private static readonly DiagnosticDescriptor Rule = DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSyntaxNodeActionInNonGenerated(
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
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(Rule, propertyDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.PropertyDeclaration);

        private static bool HasAttributes(SyntaxList<AccessorDeclarationSyntax> accessors) =>
            accessors.Any(a => a.AttributeLists.Any());

        private static bool HasDifferentModifiers(SyntaxList<AccessorDeclarationSyntax> accessors)
        {
            var accessor1 = accessors.First();
            var modifiers = ModifierKinds(accessor1).ToHashSet();

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
                ? expressionBody.ChildNodes().ElementAt(0) as AssignmentExpressionSyntax
                : null;
        }

        private static IFieldSymbol FieldSymbol(ExpressionSyntax expression, INamedTypeSymbol declaringType, SemanticModel semanticModel)
        {
            if (expression is IdentifierNameSyntax)
            {
                return semanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol;
            }

            if (!(expression is MemberAccessExpressionSyntax memberAccess)
                || !memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                return null;
            }
            else if (memberAccess.Expression is ThisExpressionSyntax)
            {
                return semanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol;
            }
            else if (!(memberAccess.Expression is IdentifierNameSyntax identifier))
            {
                return null;
            }
            else if (!(semanticModel.GetSymbolInfo(identifier).Symbol is INamedTypeSymbol type)
                     || !type.Equals(declaringType))
            {
                return null;
            }

            return semanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol;
        }

        private static IFieldSymbol FieldFromGetter(AccessorDeclarationSyntax getter, SemanticModel semanticModel)
        {
            var returnedExpression = GetReturnExpressionFromBody(getter.Body) ?? GetReturnExpressionFromExpressionBody(getter.ExpressionBody());

            return returnedExpression == null
                   ? null
                   : FieldSymbol(returnedExpression, semanticModel.GetDeclaredSymbol(getter).ContainingType, semanticModel);

            ExpressionSyntax GetReturnExpressionFromBody(BlockSyntax body) =>
                body != null && body.Statements.Count == 1 && body.Statements[0] is ReturnStatementSyntax returnStatement
                ? returnStatement.Expression
                : null;

            ExpressionSyntax GetReturnExpressionFromExpressionBody(ArrowExpressionClauseSyntax expressionBody) =>
                expressionBody?.Expression;
        }
    }
}
