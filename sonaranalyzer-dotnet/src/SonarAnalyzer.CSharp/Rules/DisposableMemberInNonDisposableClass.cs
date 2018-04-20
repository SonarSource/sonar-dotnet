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
    public class DisposableMemberInNonDisposableClass : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2931";
        private const string MessageFormat = "Implement 'IDisposable' in this class and use the 'Dispose' method to call 'Dispose' on {0}.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<Accessibility> Accessibilities = new HashSet<Accessibility>
        {
            Accessibility.Protected,
            Accessibility.Private
        };

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterCompilationStartAction(
                analysisContext =>
                {

                    var fieldsByNamedType = MultiValueDictionary<INamedTypeSymbol, IFieldSymbol>.Create<HashSet<IFieldSymbol>>();
                    var fieldsAssigned = new HashSet<IFieldSymbol>();

                    analysisContext.RegisterSymbolAction(
                        c =>
                        {
                            var namedTypeSymbol = (INamedTypeSymbol)c.Symbol;
                            if (!namedTypeSymbol.IsClass() ||
                                namedTypeSymbol.Implements(KnownType.System_IDisposable))
                            {
                                return;
                            }

                            var disposableFields = namedTypeSymbol.GetMembers()
                                .OfType<IFieldSymbol>()
                                .Where(IsNonStaticNonPublicDisposableField)
                                .ToHashSet();

                            fieldsByNamedType.AddRangeWithKey(namedTypeSymbol, disposableFields);
                        },
                        SymbolKind.NamedType);


                    analysisContext.RegisterSyntaxNodeAction(
                        c =>
                        {
                            var assignment = (AssignmentExpressionSyntax)c.Node;
                            var expression = assignment.Right;
                            var fieldSymbol = c.SemanticModel.GetSymbolInfo(assignment.Left).Symbol as IFieldSymbol;

                            AddFieldIfNeeded(fieldSymbol, expression, fieldsAssigned);
                        },
                        SyntaxKind.SimpleAssignmentExpression);

                    analysisContext.RegisterSyntaxNodeAction(
                        c =>
                        {
                            var field = (FieldDeclarationSyntax)c.Node;

                            foreach (var variableDeclaratorSyntax in field.Declaration.Variables
                                .Where(declaratorSyntax => declaratorSyntax.Initializer != null))
                            {
                                var fieldSymbol = c.SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol;

                                AddFieldIfNeeded(fieldSymbol, variableDeclaratorSyntax.Initializer.Value, fieldsAssigned);
                            }

                        },
                        SyntaxKind.FieldDeclaration);

                    analysisContext.RegisterCompilationEndAction(
                        c =>
                        {
                            foreach (var kv in fieldsByNamedType)
                            {
                                foreach (var classSyntax in kv.Key.DeclaringSyntaxReferences
                                    .Select(declaringSyntaxReference => declaringSyntaxReference.GetSyntax())
                                    .OfType<ClassDeclarationSyntax>())
                                {
                                    var assignedFields = kv.Value.Intersect(fieldsAssigned).ToList();

                                    if (!assignedFields.Any())
                                    {
                                        continue;
                                    }
                                    var variableNames = string.Join(", ",
                                        assignedFields.Select(symbol => $"'{symbol.Name}'").OrderBy(s => s));

                                    c.ReportDiagnosticIfNonGenerated(
                                        Diagnostic.Create(rule, classSyntax.Identifier.GetLocation(), variableNames),
                                        c.Compilation);
                                }
                            }
                        });
                });
        }

        private static void AddFieldIfNeeded(IFieldSymbol fieldSymbol, ExpressionSyntax expression,
            HashSet<IFieldSymbol> fieldsAssigned)
        {
            var objectCreation = expression as ObjectCreationExpressionSyntax;
            if (objectCreation == null ||
                !IsNonStaticNonPublicDisposableField(fieldSymbol))
            {
                return;
            }

            fieldsAssigned.Add(fieldSymbol);
        }

        internal static bool IsNonStaticNonPublicDisposableField(IFieldSymbol fieldSymbol)
        {
            return fieldSymbol != null &&
                   !fieldSymbol.IsStatic &&
                   Accessibilities.Contains(fieldSymbol.DeclaredAccessibility) &&
                   fieldSymbol.Type.Implements(KnownType.System_IDisposable);
        }
    }
}
