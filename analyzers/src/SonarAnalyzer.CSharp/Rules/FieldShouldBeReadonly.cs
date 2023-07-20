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

using FieldTuple = SonarAnalyzer.Common.NodeSymbolAndModel<Microsoft.CodeAnalysis.CSharp.Syntax.VariableDeclaratorSyntax, Microsoft.CodeAnalysis.IFieldSymbol>;
using TypeDeclarationTuple = SonarAnalyzer.Common.NodeAndModel<Microsoft.CodeAnalysis.CSharp.Syntax.TypeDeclarationSyntax>;

namespace SonarAnalyzer.Rules.CSharp
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class FieldShouldBeReadonly : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S2933";
        private const string MessageFormat = "Make '{0}' 'readonly'.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ISet<SyntaxKind> AssignmentKinds = new HashSet<SyntaxKind>
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
            SyntaxKind.RightShiftAssignmentExpression,
            SyntaxKindEx.CoalesceAssignmentExpression,
            SyntaxKindEx.UnsignedRightShiftAssignmentExpression
        };

        private static readonly ISet<SyntaxKind> PrefixUnaryKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.PreDecrementExpression,
            SyntaxKind.PreIncrementExpression
        };

        private static readonly ISet<SyntaxKind> PostfixUnaryKinds = new HashSet<SyntaxKind>
        {
            SyntaxKind.PostDecrementExpression,
            SyntaxKind.PostIncrementExpression
        };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var declaredSymbol = (INamedTypeSymbol)c.Symbol;
                    if (!declaredSymbol.IsClassOrStruct()
                        // Serializable classes are ignored because the serialized fields
                        // cannot be readonly. [Nonserialized] fields could be readonly,
                        // but all fields with attribute are ignored in the ReadonlyFieldCollector.
                        || declaredSymbol.HasAttribute(KnownType.System_SerializableAttribute)
                        // Partial classes are not processed.
                        // See https://github.com/dotnet/roslyn/issues/3748
                        || declaredSymbol.DeclaringSyntaxReferences.Length > 1)
                    {
                        return;
                    }

                    var partialDeclarations = declaredSymbol.DeclaringSyntaxReferences
                        .Select(reference => reference.GetSyntax())
                        .OfType<TypeDeclarationSyntax>()
                        .Select(x => new TypeDeclarationTuple(c.Compilation.GetSemanticModel(x.SyntaxTree), x))
                        .Where(x => x.Model != null);

                    var fieldCollector = new ReadonlyFieldCollector(partialDeclarations);

                    foreach (var field in fieldCollector.NonCompliantFields)
                    {
                        var identifier = field.Node.Identifier;
                        c.ReportIssue(CreateDiagnostic(Rule, identifier.GetLocation(), identifier.ValueText));
                    }
                },
                SymbolKind.NamedType);

        private sealed class ReadonlyFieldCollector
        {
            private readonly ISet<IFieldSymbol> assignedAsReadonly;
            private readonly ISet<IFieldSymbol> excludedFields;
            private readonly List<FieldTuple> allFields = new();

            public IEnumerable<FieldTuple> NonCompliantFields
            {
                get
                {
                    var reportedFields = new HashSet<IFieldSymbol>(assignedAsReadonly.Except(excludedFields));
                    return allFields.Where(f => reportedFields.Contains(f.Symbol));
                }
            }

            public ReadonlyFieldCollector(IEnumerable<TypeDeclarationTuple> partialTypeDeclarations)
            {
                excludedFields = new HashSet<IFieldSymbol>();
                assignedAsReadonly = new HashSet<IFieldSymbol>();

                foreach (var partialTypeDeclaration in partialTypeDeclarations)
                {
                    var p = new PartialTypeDeclarationProcessor(partialTypeDeclaration, this);
                    p.CollectFields();
                    allFields.AddRange(p.AllFields);
                }

                foreach (var attributedField in allFields.Where(f => ShouldBeExcluded(f)))
                {
                    excludedFields.Add(attributedField.Symbol);
                }

                static bool ShouldBeExcluded(FieldTuple fieldTuple) =>
                    fieldTuple.Symbol.GetAttributes().Any()
                    || (fieldTuple.Symbol.Type.IsStruct() && fieldTuple.Symbol.Type.SpecialType == SpecialType.None);
            }

            private sealed class PartialTypeDeclarationProcessor
            {
                private readonly TypeDeclarationTuple partialTypeDeclaration;
                private readonly ReadonlyFieldCollector readonlyFieldCollector;

                public IEnumerable<FieldTuple> AllFields { get; }

                public PartialTypeDeclarationProcessor(TypeDeclarationTuple partialTypeDeclaration, ReadonlyFieldCollector readonlyFieldCollector)
                {
                    this.partialTypeDeclaration = partialTypeDeclaration;
                    this.readonlyFieldCollector = readonlyFieldCollector;

                    AllFields = partialTypeDeclaration.Node.DescendantNodes()
                        .OfType<FieldDeclarationSyntax>()
                        .SelectMany(f => GetAllFields(f));
                }

                private IEnumerable<FieldTuple> GetAllFields(FieldDeclarationSyntax fieldDeclaration) =>
                    fieldDeclaration.Declaration.Variables
                        .Select(x => new FieldTuple(partialTypeDeclaration.Model, x, partialTypeDeclaration.Model.GetDeclaredSymbol(x) as IFieldSymbol));

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
                        IsFieldRelevant(f.Symbol)
                        && f.Node.Initializer != null);

                    foreach (var field in fieldDeclarations)
                    {
                        readonlyFieldCollector.assignedAsReadonly.Add(field.Symbol);
                    }
                }

                private void CollectFieldsFromArguments()
                {
                    var arguments = partialTypeDeclaration.Node.DescendantNodes()
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
                    var postfixUnaries = partialTypeDeclaration.Node.DescendantNodes()
                        .OfType<PostfixUnaryExpressionSyntax>()
                        .Where(a => PostfixUnaryKinds.Contains(a.Kind()));

                    foreach (var postfixUnary in postfixUnaries)
                    {
                        ProcessExpression(postfixUnary.Operand);
                    }
                }

                private void CollectFieldsFromPrefixUnaryExpressions()
                {
                    var prefixUnaries = partialTypeDeclaration.Node.DescendantNodes()
                        .OfType<PrefixUnaryExpressionSyntax>()
                        .Where(a => PrefixUnaryKinds.Contains(a.Kind()));

                    foreach (var prefixUnary in prefixUnaries)
                    {
                        ProcessExpression(prefixUnary.Operand);
                    }
                }

                private void CollectFieldsFromAssignments()
                {
                    var assignments = partialTypeDeclaration.Node.DescendantNodes()
                        .OfType<AssignmentExpressionSyntax>()
                        .Where(a => AssignmentKinds.Contains(a.Kind()));

                    foreach (var assignment in assignments)
                    {
                        var leftSideExpressions = TupleExpressionsOrSelf(assignment.Left);
                        foreach (var leftSideExpression in leftSideExpressions)
                        {
                            ProcessExpression(leftSideExpression);
                        }
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

                    var fieldSymbol = partialTypeDeclaration.Model.GetSymbolInfo(topExpression).Symbol as IFieldSymbol;
                    if (fieldSymbol?.Type == null
                        || !fieldSymbol.Type.IsValueType)
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

                    if (noParens is not MemberAccessExpressionSyntax memberAccess)
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
                    var fieldSymbol = partialTypeDeclaration.Model.GetSymbolInfo(expression).Symbol as IFieldSymbol;
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
                        readonlyFieldCollector.excludedFields.Add(fieldSymbol);
                        return;
                    }

                    if (partialTypeDeclaration.Model.GetEnclosingSymbol(expression.SpanStart) is not IMethodSymbol methodSymbol)
                    {
                        readonlyFieldCollector.excludedFields.Add(fieldSymbol);
                        return;
                    }

                    if (methodSymbol.MethodKind == MethodKind.Constructor
                        && methodSymbol.ContainingType.Equals(fieldSymbol.ContainingType))
                    {
                        readonlyFieldCollector.assignedAsReadonly.Add(fieldSymbol);
                    }
                    else
                    {
                        readonlyFieldCollector.excludedFields.Add(fieldSymbol);
                    }
                }

                private static bool IsFieldRelevant(IFieldSymbol fieldSymbol) =>
                    fieldSymbol is { IsStatic: false, IsConst: false, IsReadOnly: false, DeclaredAccessibility: Accessibility.Private };

                private static IEnumerable<ExpressionSyntax> TupleExpressionsOrSelf(ExpressionSyntax expression)
                {
                    if (TupleExpressionSyntaxWrapper.IsInstance(expression))
                    {
                        var assignments = new List<ExpressionSyntax>();
                        foreach (var argument in ((TupleExpressionSyntaxWrapper)expression).Arguments)
                        {
                            assignments.Add(argument.Expression);
                        }
                        return assignments;
                    }
                    else
                    {
                        return new List<ExpressionSyntax> { expression };
                    }
                }
            }
        }
    }
}
