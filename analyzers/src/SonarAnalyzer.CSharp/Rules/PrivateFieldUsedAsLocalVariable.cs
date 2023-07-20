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
    public sealed class PrivateFieldUsedAsLocalVariable : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S1450";
        private const string MessageFormat = "Remove the field '{0}' and declare it as a local variable in the relevant methods.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        private static readonly ISet<SyntaxKind> NonPrivateModifiers = new HashSet<SyntaxKind>
        {
            SyntaxKind.PublicKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword,
        };

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                    if (c.IsRedundantPositionalRecordContext() || typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                    {
                        return;
                    }

                    var methodNames = typeDeclaration.Members.OfType<MethodDeclarationSyntax>().Select(x => x.Identifier.ValueText).ToHashSet();
                    var privateFields = GetPrivateFields(c.SemanticModel, typeDeclaration);
                    var collector = new FieldAccessCollector(c.SemanticModel, privateFields, methodNames);
                    if (!collector.SafeVisit(typeDeclaration))
                    {
                        // We couldn't finish the exploration so we cannot take any decision
                        return;
                    }

                    foreach (var fieldSymbol in privateFields.Keys.Where(collector.IsRemovableField))
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, privateFields[fieldSymbol].GetLocation(), fieldSymbol.Name));
                    }
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordClassDeclaration,
                SyntaxKindEx.RecordStructDeclaration);

        private static IDictionary<IFieldSymbol, VariableDeclaratorSyntax> GetPrivateFields(SemanticModel semanticModel, TypeDeclarationSyntax typeDeclaration)
        {
            return typeDeclaration.Members
                .OfType<FieldDeclarationSyntax>()
                .Where(IsPrivate)
                .Where(HasNoAttributes)
                .SelectMany(f => f.Declaration.Variables)
                .ToDictionary(
                    variable => (IFieldSymbol)semanticModel.GetDeclaredSymbol(variable),
                    variable => variable);

            bool IsPrivate(FieldDeclarationSyntax fieldDeclaration) =>
                !fieldDeclaration.Modifiers.Select(m => m.Kind()).Any(NonPrivateModifiers.Contains);

            bool HasNoAttributes(FieldDeclarationSyntax fieldDeclaration) =>
                fieldDeclaration.AttributeLists.Count == 0;
        }

        private sealed class FieldAccessCollector : SafeCSharpSyntaxWalker
        {
            // Contains statements that READ field values. First grouped by field symbol (that is read),
            // then by method/property/ctor symbol (that contains the statements)
            private readonly Dictionary<IFieldSymbol, Lookup<ISymbol, SyntaxNode>> readsByField = new();

            // Contains statements that WRITE field values. First grouped by field symbol (that is written),
            // then by method/property/ctor symbol (that contains the statements)
            private readonly Dictionary<IFieldSymbol, Lookup<ISymbol, SyntaxNode>> writesByField = new();

            // Contains all method/property invocations grouped by the statement that contains them.
            private readonly Lookup<SyntaxNode, ISymbol> invocations = new();

            private readonly SemanticModel semanticModel;
            private readonly IDictionary<IFieldSymbol, VariableDeclaratorSyntax> privateFields;

            private readonly HashSet<string> methodNames;

            public FieldAccessCollector(SemanticModel semanticModel, IDictionary<IFieldSymbol, VariableDeclaratorSyntax> privateFields, HashSet<string> methodNames)
            {
                this.semanticModel = semanticModel;
                this.privateFields = privateFields;
                this.methodNames = methodNames;
            }

            public bool IsRemovableField(IFieldSymbol fieldSymbol)
            {
                var writesByEnclosingSymbol = writesByField.GetValueOrDefault(fieldSymbol);
                var readsByEnclosingSymbol = readsByField.GetValueOrDefault(fieldSymbol);

                // No methods overwrite the field value
                if (writesByEnclosingSymbol == null)
                {
                    return false;
                }

                // A field is removable when no method reads it, or all methods that read it, overwrite it before reading
                // However, as S4487 reports on fields that are written but not read, we only raise on the latter case
                return readsByEnclosingSymbol?.Keys.All(ValueOverwrittenBeforeReading) ?? false;

                bool ValueOverwrittenBeforeReading(ISymbol enclosingSymbol)
                {
                    var writeStatements = writesByEnclosingSymbol.GetValueOrDefault(enclosingSymbol);
                    var readStatements = readsByEnclosingSymbol.GetValueOrDefault(enclosingSymbol);

                    // Note that Enumerable.All() will return true if readStatements is empty. The collection
                    // will be empty if the field is read only in property/field initializers or returned from
                    // expression-bodied methods.
                    return writeStatements != null && (readStatements?.All(IsPrecededWithWrite) ?? false);

                    // Returns true when readStatement is preceded with a statement that overwrites fieldSymbol,
                    // or false when readStatement is preceded with an invocation of a method or property that
                    // overwrites fieldSymbol.
                    bool IsPrecededWithWrite(SyntaxNode readStatementOrArrowExpression)
                    {
                        if (readStatementOrArrowExpression is StatementSyntax readStatement)
                        {
                            foreach (var statement in readStatement.GetPreviousStatements())
                            {
                                // When the readStatement is preceded with a write statement (that is also not a read statement)
                                // we want to report this field.
                                if (IsOverwritingValue(statement))
                                {
                                    return true;
                                }

                                // When the readStatement is preceded with an invocation that has side effects, e.g. writes the field
                                // we don't want to report this field because it could be difficult or impossible to change the code.
                                if (IsInvocationWithSideEffects(statement))
                                {
                                    return false;
                                }
                            }
                        }
                        // ArrowExpressionClauseSyntax cannot be preceded by anything...
                        return false;
                    }

                    bool IsOverwritingValue(StatementSyntax statement) =>
                        writeStatements.Contains(statement)
                        && !readStatements.Contains(statement);

                    bool IsInvocationWithSideEffects(StatementSyntax statement) =>
                        invocations.TryGetValue(statement, out var invocationsInStatement)
                        && invocationsInStatement.Any(writesByEnclosingSymbol.ContainsKey);
                }
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                if (privateFields.Keys.Any(x => x.Name == node.Identifier.ValueText))
                {
                    var memberReference = GetTopmostSyntaxWithTheSameSymbol(node);
                    if (memberReference.Symbol is IFieldSymbol fieldSymbol
                        && privateFields.ContainsKey(fieldSymbol))
                    {
                        ClassifyFieldReference(semanticModel.GetEnclosingSymbol(memberReference.Node.SpanStart), memberReference);
                    }
                }
                base.VisitIdentifierName(node);
            }

            public override void VisitInvocationExpression(InvocationExpressionSyntax node)
            {
                if (node.GetMethodCallIdentifier() is { } methodCallIdentifier
                    && methodNames.Contains(methodCallIdentifier.ValueText)
                    && GetTopmostSyntaxWithTheSameSymbol(node) is var memberReference
                    && memberReference.Symbol is IMethodSymbol
                    && GetParentPseudoStatement(memberReference) is { } pseudoStatement)
                {
                    invocations.GetOrAdd(pseudoStatement, _ => new HashSet<ISymbol>())
                        .Add(memberReference.Symbol);
                }
                base.VisitInvocationExpression(node);
            }

            // A PseudoStatement is a Statement or an ArrowExpressionClauseSyntax (which denotes an expression-bodied member).
            private static SyntaxNode GetParentPseudoStatement(NodeAndSymbol memberReference) =>
                memberReference.Node.Ancestors().FirstOrDefault(a => a is StatementSyntax or ArrowExpressionClauseSyntax);

            /// <summary>
            /// Stores the statement that contains the provided field reference in one of the "reads" or "writes" collections,
            /// first grouped by field symbol, then by containing method.
            /// </summary>
            private void ClassifyFieldReference(ISymbol enclosingSymbol, NodeAndSymbol fieldReference)
            {
                // It is important to create the field access HashSet regardless of the statement (see the local var below)
                // being null or not, because the rule will not be able to detect field reads from inline property
                // or field initializers.
                var fieldAccessInMethod = (IsWrite(fieldReference) ? writesByField : readsByField)
                    .GetOrAdd((IFieldSymbol)fieldReference.Symbol, x => new Lookup<ISymbol, SyntaxNode>())
                    .GetOrAdd(enclosingSymbol, x => new HashSet<SyntaxNode>());

                var pseudoStatement = GetParentPseudoStatement(fieldReference);
                if (pseudoStatement != null)
                {
                    fieldAccessInMethod.Add(pseudoStatement);
                }
            }

            private static bool IsWrite(NodeAndSymbol fieldReference)
            {
                // If the field is not static and is not from the current instance we
                // consider the reference as read.
                if (!fieldReference.Symbol.IsStatic
                    && !(fieldReference.Node as ExpressionSyntax).RemoveParentheses().IsOnThis())
                {
                    return false;
                }

                return IsLeftSideOfAssignment(fieldReference.Node)
                    || IsOutArgument(fieldReference.Node);

                bool IsOutArgument(SyntaxNode syntaxNode) =>
                    syntaxNode.Parent is ArgumentSyntax argument
                    && argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);

                bool IsLeftSideOfAssignment(SyntaxNode syntaxNode) =>
                    syntaxNode.Parent.IsKind(SyntaxKind.SimpleAssignmentExpression)
                    && syntaxNode.Parent is AssignmentExpressionSyntax assignmentExpression
                    && assignmentExpression.Left == syntaxNode;
            }

            private NodeAndSymbol GetTopmostSyntaxWithTheSameSymbol(SyntaxNode identifier) =>
                // All of the cases below could be parts of invocation or other expressions
                identifier.Parent switch
                {
                    // this.identifier or a.identifier or ((a)).identifier, but not identifier.other
                    MemberAccessExpressionSyntax memberAccess when memberAccess.Name == identifier =>
                        new NodeAndSymbol(memberAccess.GetSelfOrTopParenthesizedExpression(), semanticModel.GetSymbolInfo(memberAccess).Symbol),
                    // this?.identifier or a?.identifier or ((a))?.identifier, but not identifier?.other
                    MemberBindingExpressionSyntax memberBinding when memberBinding.Name == identifier =>
                        new NodeAndSymbol(memberBinding.Parent.GetSelfOrTopParenthesizedExpression(), semanticModel.GetSymbolInfo(memberBinding).Symbol),
                    // identifier or ((identifier))
                    _ => new NodeAndSymbol(identifier.GetSelfOrTopParenthesizedExpression(), semanticModel.GetSymbolInfo(identifier).Symbol)
                };
        }

        private sealed class Lookup<TKey, TElement> : Dictionary<TKey, HashSet<TElement>> { }
    }
}
