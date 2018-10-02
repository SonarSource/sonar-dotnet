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
    using FieldTuple = SyntaxNodeSymbolSemanticModelTuple<VariableDeclaratorSyntax, IFieldSymbol>;
    using TypeDeclarationTuple = SyntaxNodeAndSemanticModel<TypeDeclarationSyntax>;

    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    [Rule(DiagnosticId)]
    public sealed class FieldShouldBeReadonly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2933";
        private const string MessageFormat = "Make '{0}' 'readonly'.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> assignmentKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.SimpleAssignmentExpression,
            SyntaxKind.AddAssignmentExpression,
            SyntaxKind.SubtractAssignmentExpression,
            SyntaxKind.MultiplyAssignmentExpression,
            SyntaxKind.DivideAssignmentExpression,
            SyntaxKind.ModuloAssignmentExpression,
            SyntaxKind.AndAssignmentExpression,
            SyntaxKind.ExclusiveOrAssignmentExpression,
            SyntaxKind.OrAssignmentExpression,
            SyntaxKind.LeftShiftAssignmentExpression,
            SyntaxKind.RightShiftAssignmentExpression
        };

        private static readonly ISet<SyntaxKind> prefixUnaryKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.PreDecrementExpression,
            SyntaxKind.PreIncrementExpression
        };

        private static readonly ISet<SyntaxKind> postfixUnaryKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PostIncrementExpression
        };

        protected override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSymbolAction(
                c =>
                {
                    var declaredSymbol = (INamedTypeSymbol)c.Symbol;
                    if (!declaredSymbol.IsClassOrStruct() ||
                        // Serializable classes are ignored because the serialized fields
                        // cannot be readonly. [Nonserialized] fields could be readonly,
                        // but all fields with attribute are ignored in the ReadonlyFieldCollector.
                        declaredSymbol.GetAttributes(KnownType.System_SerializableAttribute).Any())
                    {
                        return;
                    }

                    if (declaredSymbol.DeclaringSyntaxReferences.Length > 1)
                    {
                        // Partial classes are not processed.
                        // See https://github.com/dotnet/roslyn/issues/3748
                        return;
                    }

                    var partialDeclarations = declaredSymbol.DeclaringSyntaxReferences
                        .Select(reference => reference.GetSyntax())
                        .OfType<TypeDeclarationSyntax>()
                        .Select(node =>
                            new SyntaxNodeAndSemanticModel<TypeDeclarationSyntax>
                            {
                                SyntaxNode = node,
                                SemanticModel = c.Compilation.GetSemanticModel(node.SyntaxTree)
                            })
                        .Where(n => n.SemanticModel != null);

                    var fieldCollector = new ReadonlyFieldCollector(partialDeclarations);

                    foreach (var field in fieldCollector.NonCompliantFields)
                    {
                        var identifier = field.SyntaxNode.Identifier;
                        c.ReportDiagnosticIfNonGenerated(Diagnostic.Create(rule, identifier.GetLocation(), identifier.ValueText));
                    }
                },
                SymbolKind.NamedType);
        }

        private class ReadonlyFieldCollector
        {
            private readonly ISet<IFieldSymbol> assignedAsReadonly;
            private readonly ISet<IFieldSymbol> excludedFields;
            private readonly List<FieldTuple> allFields = new List<FieldTuple>();

            public IEnumerable<FieldTuple> NonCompliantFields
            {
                get
                {
                    var reportedFields = new HashSet<IFieldSymbol>(this.assignedAsReadonly.Except(this.excludedFields));
                    return this.allFields.Where(f => reportedFields.Contains(f.Symbol));
                }
            }

            public ReadonlyFieldCollector(IEnumerable<TypeDeclarationTuple> partialTypeDeclarations)
            {
                this.excludedFields = new HashSet<IFieldSymbol>();
                this.assignedAsReadonly = new HashSet<IFieldSymbol>();

                foreach (var partialTypeDeclaration in partialTypeDeclarations)
                {
                    var p = new PartialTypeDeclarationProcessor(partialTypeDeclaration, this);
                    p.CollectFields();
                    this.allFields.AddRange(p.AllFields);
                }

                foreach (var attributedField in this.allFields.Where(f => f.Symbol.GetAttributes().Any()))
                {
                    this.excludedFields.Add(attributedField.Symbol);
                }
            }

            private class PartialTypeDeclarationProcessor
            {
                private readonly TypeDeclarationTuple partialTypeDeclaration;
                private readonly ReadonlyFieldCollector readonlyFieldCollector;

                public IEnumerable<FieldTuple> AllFields { get; }

                public PartialTypeDeclarationProcessor(TypeDeclarationTuple partialTypeDeclaration, ReadonlyFieldCollector readonlyFieldCollector)
                {
                    this.partialTypeDeclaration = partialTypeDeclaration;
                    this.readonlyFieldCollector = readonlyFieldCollector;

                    AllFields = partialTypeDeclaration.SyntaxNode.DescendantNodes()
                        .OfType<FieldDeclarationSyntax>()
                        .SelectMany(f => GetAllFields(f));
                }

                private IEnumerable<FieldTuple> GetAllFields(FieldDeclarationSyntax fieldDeclaration)
                {
                    return fieldDeclaration.Declaration.Variables
                        .Select(variableDeclaratorSyntax => new FieldTuple
                        {
                            SyntaxNode = variableDeclaratorSyntax,
                            Symbol = this.partialTypeDeclaration.SemanticModel.GetDeclaredSymbol(variableDeclaratorSyntax) as IFieldSymbol,
                            SemanticModel = this.partialTypeDeclaration.SemanticModel
                        });
                }

                public void CollectFields()
                {
                    CollectFieldsFromDeclarations();
                    CollectFieldsFromAssignments();
                    CollectFieldsFromPrefixUnaryExpressions();
                    CollectFieldsFromPostfixUnaryExpressions();
                    CollectFieldsFromArguments();
                }

                private void CollectFieldsFromDeclarations()
                {
                    var fieldDeclarations = AllFields.Where(f =>
                        IsFieldRelevant(f.Symbol) &&
                        f.SyntaxNode.Initializer != null);

                    foreach (var field in fieldDeclarations)
                    {
                        this.readonlyFieldCollector.assignedAsReadonly.Add(field.Symbol);
                    }
                }

                private void CollectFieldsFromArguments()
                {
                    var arguments = this.partialTypeDeclaration.SyntaxNode.DescendantNodes()
                        .OfType<ArgumentSyntax>()
                        .Where(a => !a.RefOrOutKeyword.IsKind(SyntaxKind.None));

                    foreach (var argument in arguments)
                    {
                        // ref/out should be handled the same way as all other field assignments:
                        ProcessExpression(argument.Expression);
                    }
                }

                private void CollectFieldsFromPostfixUnaryExpressions()
                {
                    var postfixUnaries = this.partialTypeDeclaration.SyntaxNode.DescendantNodes()
                        .OfType<PostfixUnaryExpressionSyntax>()
                        .Where(a => postfixUnaryKinds.Contains(a.Kind()));

                    foreach (var postfixUnary in postfixUnaries)
                    {
                        ProcessExpression(postfixUnary.Operand);
                    }
                }

                private void CollectFieldsFromPrefixUnaryExpressions()
                {
                    var prefixUnaries = this.partialTypeDeclaration.SyntaxNode.DescendantNodes()
                        .OfType<PrefixUnaryExpressionSyntax>()
                        .Where(a => prefixUnaryKinds.Contains(a.Kind()));

                    foreach (var prefixUnary in prefixUnaries)
                    {
                        ProcessExpression(prefixUnary.Operand);
                    }
                }

                private void CollectFieldsFromAssignments()
                {
                    var assignments = this.partialTypeDeclaration.SyntaxNode.DescendantNodes()
                        .OfType<AssignmentExpressionSyntax>()
                        .Where(a => assignmentKinds.Contains(a.Kind()));

                    foreach (var assignment in assignments)
                    {
                        ProcessExpression(assignment.Left);
                    }
                }

                private void ProcessExpression(ExpressionSyntax expression)
                {
                    ProcessAssignedExpression(expression);
                    ProcessAssignedTopMemberAccessExpression(expression);
                }

                private void ProcessAssignedTopMemberAccessExpression(ExpressionSyntax expression)
                {
                    var topExpression = GetTopMemberAccessIfNested(expression);
                    if (topExpression == null)
                    {
                        return;
                    }

                    var fieldSymbol = this.partialTypeDeclaration.SemanticModel.GetSymbolInfo(topExpression).Symbol as IFieldSymbol;
                    if (fieldSymbol?.Type == null ||
                        !fieldSymbol.Type.IsValueType)
                    {
                        return;
                    }

                    ProcessExpressionOnField(topExpression, fieldSymbol);
                }

                private static ExpressionSyntax GetTopMemberAccessIfNested(ExpressionSyntax expression, bool isNestedMemberAccess = false)
                {
                    // If expression is (this.a.b).c, we need to return this.a

                    var noParens = expression.RemoveParentheses();

                    if (noParens is NameSyntax)
                    {
                        return isNestedMemberAccess ? noParens : null;
                    }

                    if (!(noParens is MemberAccessExpressionSyntax memberAccess))
                    {
                        return null;
                    }

                    if (memberAccess.Expression.RemoveParentheses().IsKind(SyntaxKind.ThisExpression))
                    {
                        return isNestedMemberAccess ? memberAccess : null;
                    }

                    return GetTopMemberAccessIfNested(memberAccess.Expression, true);
                }

                private void ProcessAssignedExpression(ExpressionSyntax expression)
                {
                    var fieldSymbol = this.partialTypeDeclaration.SemanticModel.GetSymbolInfo(expression).Symbol as IFieldSymbol;
                    ProcessExpressionOnField(expression, fieldSymbol);
                }

                private void ProcessExpressionOnField(ExpressionSyntax expression, IFieldSymbol fieldSymbol)
                {
                    if (!IsFieldRelevant(fieldSymbol))
                    {
                        return;
                    }

                    if (!expression.RemoveParentheses().IsOnThis())
                    {
                        this.readonlyFieldCollector.excludedFields.Add(fieldSymbol);
                        return;
                    }

                    if (!(this.partialTypeDeclaration.SemanticModel.GetEnclosingSymbol(expression.SpanStart) is IMethodSymbol constructorSymbol))
                    {
                        this.readonlyFieldCollector.excludedFields.Add(fieldSymbol);
                        return;
                    }

                    if (constructorSymbol.MethodKind == MethodKind.Constructor &&
                        constructorSymbol.ContainingType.Equals(fieldSymbol.ContainingType))
                    {
                        this.readonlyFieldCollector.assignedAsReadonly.Add(fieldSymbol);
                    }
                    else
                    {
                        this.readonlyFieldCollector.excludedFields.Add(fieldSymbol);
                    }
                }

                private static bool IsFieldRelevant(IFieldSymbol fieldSymbol)
                {
                    return fieldSymbol != null &&
                           !fieldSymbol.IsStatic &&
                           !fieldSymbol.IsConst &&
                           !fieldSymbol.IsReadOnly &&
                           fieldSymbol.DeclaredAccessibility == Accessibility.Private;
                }
            }
        }
    }
}
