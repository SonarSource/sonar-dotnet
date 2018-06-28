/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2018 SonarSource SA
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

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public class PropertyToAutoProperty : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2292";
        private const string MessageFormat = "Make this an auto-implemented property and remove its backing field.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var propertyDeclaration = (PropertyDeclarationSyntax)c.Node;
                    var propertySymbol = c.SemanticModel.GetDeclaredSymbol(propertyDeclaration);
                    if (propertyDeclaration.AccessorList == null ||
                        propertyDeclaration.AccessorList.Accessors.Count != 2 ||
                        propertySymbol == null ||
                        HasDifferentModifiers(propertyDeclaration.AccessorList.Accessors) ||
                        HasAttributes(propertyDeclaration.AccessorList.Accessors))
                    {
                        return;
                    }

                    var accessors = propertyDeclaration.AccessorList.Accessors;
                    var getter = accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.GetAccessorDeclaration));
                    var setter = accessors.FirstOrDefault(a => a.IsKind(SyntaxKind.SetAccessorDeclaration));

                    if (getter == null || setter == null)
                    {
                        return;
                    }
                    if (TryGetFieldFromGetter(getter, c.SemanticModel, out var getterField) &&
                        TryGetFieldFromSetter(setter, c.SemanticModel, out var setterField) &&
                        getterField.Equals(setterField) &&
                        !getterField.GetAttributes().Any() &&
                        getterField.IsStatic == propertySymbol.IsStatic &&
                        getterField.Type.Equals(propertySymbol.Type))
                    {
                        c.ReportDiagnosticWhenActive(Diagnostic.Create(rule, propertyDeclaration.Identifier.GetLocation()));
                    }
                },
                SyntaxKind.PropertyDeclaration);
        }

        private static bool HasAttributes(SyntaxList<AccessorDeclarationSyntax> accessors)
        {
            return accessors.Any(a => a.AttributeLists.Any());
        }

        private static bool HasDifferentModifiers(SyntaxList<AccessorDeclarationSyntax> accessors)
        {
            var accessor1 = accessors.First();
            var modifiers = GetModifierKinds(accessor1).ToHashSet();

            return accessors.Skip(1).Any(a => !modifiers.SetEquals(GetModifierKinds(a)));
        }

        private static IEnumerable<SyntaxKind> GetModifierKinds(AccessorDeclarationSyntax accessor)
        {
            return accessor.Modifiers.Select(m => m.Kind());
        }

        private static bool TryGetFieldFromSetter(AccessorDeclarationSyntax setter, SemanticModel semanticModel, out IFieldSymbol setterField)
        {
            setterField = null;
            if (setter.Body == null ||
                setter.Body.Statements.Count != 1)
            {
                return false;
            }

            var statement = setter.Body.Statements[0] as ExpressionStatementSyntax;
            if (!(statement?.Expression is AssignmentExpressionSyntax assignment) ||
                !assignment.IsKind(SyntaxKind.SimpleAssignmentExpression))
            {
                return false;
            }

            if (!(semanticModel.GetSymbolInfo(assignment.Right).Symbol is IParameterSymbol parameter) ||
                parameter.Name != "value" ||
                !parameter.IsImplicitlyDeclared)
            {
                return false;
            }

            return TryGetField(assignment.Left, semanticModel.GetDeclaredSymbol(setter).ContainingType,
                semanticModel, out setterField);
        }

        private static bool TryGetField(ExpressionSyntax expression, INamedTypeSymbol declaringType,
            SemanticModel semanticModel, out IFieldSymbol field)
        {
            if (expression is IdentifierNameSyntax)
            {
                field = semanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol;
                return field != null;
            }

            if (!(expression is MemberAccessExpressionSyntax memberAccess) ||
                !memberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
            {
                field = null;
                return false;
            }

            if (memberAccess.Expression is ThisExpressionSyntax)
            {
                field = semanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol;
                return field != null;
            }

            if (!(memberAccess.Expression is IdentifierNameSyntax identifier))
            {
                field = null;
                return false;
            }

            if (!(semanticModel.GetSymbolInfo(identifier).Symbol is INamedTypeSymbol type) ||
                !type.Equals(declaringType))
            {
                field = null;
                return false;
            }

            field = semanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol;
            return field != null;
        }

        private static bool TryGetFieldFromGetter(AccessorDeclarationSyntax getter, SemanticModel semanticModel, out IFieldSymbol getterField)
        {
            getterField = null;
            if (getter.Body == null ||
                getter.Body.Statements.Count != 1)
            {
                return false;
            }

            if (!(getter.Body.Statements[0] is ReturnStatementSyntax statement) ||
                statement.Expression == null)
            {
                return false;
            }

            return TryGetField(statement.Expression, semanticModel.GetDeclaredSymbol(getter).ContainingType,
                semanticModel, out getterField);
        }
    }
}
