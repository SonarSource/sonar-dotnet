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
    public class PrivateFieldUsedAsLocalVariable : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S1450";
        private const string MessageFormat = "Remove the field '{0}' and declare it as a local variable in the relevant methods.";

        private static readonly DiagnosticDescriptor rule =
            DiagnosticDescriptorBuilder.GetDescriptor(DiagnosticId, MessageFormat, RspecStrings.ResourceManager);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        private static readonly ISet<SyntaxKind> NonPrivateModifiers = new HashSet<SyntaxKind>
        {
            SyntaxKind.PublicKeyword,
            SyntaxKind.ProtectedKeyword,
            SyntaxKind.InternalKeyword,
        };

        protected sealed override void Initialize(SonarAnalysisContext context)
        {
            context.RegisterSyntaxNodeActionInNonGenerated(
                c =>
                {
                    var typeDeclaration = (TypeDeclarationSyntax)c.Node;

                    if (typeDeclaration.Modifiers.Any(SyntaxKind.PartialKeyword))
                    {
                        return;
                    }

                    var privateFields = GetPrivateFields(c.SemanticModel, typeDeclaration);

                    var collector = new FieldAccessCollector(c.SemanticModel, privateFields);
                    collector.Visit(typeDeclaration);

                    privateFields.Keys
                        .Where(collector.IsRemovableField)
                        .Select(CreateDiagnostic)
                        .ToList()
                        .ForEach(d => c.ReportDiagnosticWhenActive(d));

                    Diagnostic CreateDiagnostic(IFieldSymbol fieldSymbol) =>
                        Diagnostic.Create(rule, privateFields[fieldSymbol].GetLocation(), fieldSymbol.Name);
                },
                SyntaxKind.ClassDeclaration,
                SyntaxKind.StructDeclaration);
        }

        private static IDictionary<IFieldSymbol, VariableDeclaratorSyntax> GetPrivateFields(SemanticModel semanticModel,
            TypeDeclarationSyntax typeDeclaration)
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

        private class FieldAccessCollector : CSharpSyntaxWalker
        {
            // The following two dictionaries contain read/write statements correspondingly, first grouped by
            // field symbol (that is read/written by the statements), then by method symbol (that contains
            // the statements)
            private readonly Dictionary<ISymbol, Lookup<ISymbol, StatementSyntax>> reads =
                new Dictionary<ISymbol, Lookup<ISymbol, StatementSyntax>>();
            private readonly Dictionary<ISymbol, Lookup<ISymbol, StatementSyntax>> writes =
                new Dictionary<ISymbol, Lookup<ISymbol, StatementSyntax>>();
            // The following dictionary contains method and property symbol collections, grouped by
            // statement that contains them.
            private readonly Lookup<StatementSyntax, ISymbol> invocations =
                new Lookup<StatementSyntax, ISymbol>();

            private readonly SemanticModel semanticModel;
            private readonly IDictionary<IFieldSymbol, VariableDeclaratorSyntax> privateFields;

            public FieldAccessCollector(SemanticModel semanticModel, IDictionary<IFieldSymbol, VariableDeclaratorSyntax> privateFields)
            {
                this.semanticModel = semanticModel;
                this.privateFields = privateFields;
            }

            public bool IsRemovableField(IFieldSymbol fieldSymbol)
            {
                var writesByMethod = writes.GetValueOrDefault(fieldSymbol);
                var readsByMethod = reads.GetValueOrDefault(fieldSymbol);

                // No methods overwrite the field value
                if (writesByMethod == null)
                {
                    return false;
                }

                // A field is removable when no method reads it, or all methods that read it, overwrite it before reading
                return readsByMethod == null
                    || readsByMethod.Keys.All(MethodOverwritesBeforeReading);

                bool MethodOverwritesBeforeReading(ISymbol methodSymbol)
                {
                    var writeStatements = writesByMethod.GetValueOrDefault(methodSymbol);
                    var readStatements = readsByMethod.GetValueOrDefault(methodSymbol);

                    // Note that Enumerable.All() will return true if readStatements is empty. The collection
                    // will be empty if the field is read only in property/field initializers or returned from
                    // expression-bodied methods.
                    return readStatements == null
                        || writeStatements != null && readStatements.All(IsPrecededWithWrite);

                    // Returns true when readStatement is preceded with a statement that overwrites fieldSymbol,
                    // or false when readStatement is preceded with an invocation of a method or property that
                    // overwrites fieldSymbol.
                    bool IsPrecededWithWrite(StatementSyntax readStatement)
                    {
                        foreach (var statement in GetPreviousStatements(readStatement))
                        {
                            // When the readStatement is preceded with a write statement (that is also not a read statement)
                            // we want to report this field.
                            if (writeStatements.Contains(statement) && !readStatements.Contains(statement))
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

                        return false;
                    }
                }

                bool IsInvocationWithSideEffects(StatementSyntax statement) =>
                    invocations.TryGetValue(statement, out var invocationsInStatement) &&
                    invocationsInStatement.Any(writesByMethod.ContainsKey);
            }

            // Returns all statements before the specified statement within the containing method
            private static IEnumerable<StatementSyntax> GetPreviousStatements(StatementSyntax statement)
            {
                var previousStatements = statement.Parent.ChildNodes()
                    .OfType<StatementSyntax>()
                    .TakeWhile(x => x != statement)
                    .Reverse();

                return statement.Parent is StatementSyntax parentStatement
                    ? previousStatements.Union(GetPreviousStatements(parentStatement))
                    : previousStatements;
            }

            public override void VisitIdentifierName(IdentifierNameSyntax node)
            {
                var memberReference = GetTopmostSyntaxWithTheSameSymbol(node);

                if (memberReference.Symbol == null)
                {
                    return;
                }

                var enclosingSymbol = semanticModel.GetEnclosingSymbol(memberReference.Syntax.SpanStart);

                if (memberReference.Symbol is IFieldSymbol fieldSymbol &&
                    privateFields.ContainsKey(fieldSymbol))
                {
                    ClassifyFieldReference(enclosingSymbol, memberReference);
                }
                else if (memberReference.Symbol is IMethodSymbol
                    || memberReference.Symbol is IPropertySymbol)
                {
                    var statement = memberReference.Syntax.FirstAncestorOrSelf<StatementSyntax>();
                    if (statement != null)
                    {
                        var invocationsInStatement = invocations.GetOrAdd(statement, x => new HashSet<ISymbol>());
                        invocationsInStatement.Add(memberReference.Symbol);
                    }
                }
            }

            /// <summary>
            /// Stores the statement that contains the provided field reference in one of the "reads" or "writes" collections,
            /// first grouped by field symbol, then by containing method.
            /// </summary>
            private void ClassifyFieldReference(ISymbol enclosingSymbol, SyntaxNodeWithSymbol<SyntaxNode, ISymbol> fieldReference)
            {
                // It is important to create the field access HashSet regardless of the statement (local var below)
                // being null or not, because the rule will not be able to detect field reads from inline property
                // or field initializers.
                var fieldAccessInMethod = (IsWrite(fieldReference) ? writes : reads)
                    .GetOrAdd(fieldReference.Symbol, x => new Lookup<ISymbol, StatementSyntax>())
                    .GetOrAdd(enclosingSymbol, x => new HashSet<StatementSyntax>());

                var statement = fieldReference.Syntax.FirstAncestorOrSelf<StatementSyntax>();
                if (statement != null)
                {
                    fieldAccessInMethod.Add(statement);
                }
            }

            private static bool IsWrite(SyntaxNodeWithSymbol<SyntaxNode, ISymbol> fieldReference)
            {
                // If the field is not static and is not from the current instance we
                // consider the reference as read.
                if (!fieldReference.Symbol.IsStatic &&
                    !(fieldReference.Syntax as ExpressionSyntax).RemoveParentheses().IsOnThis())
                {
                    return false;
                }

                return IsLeftSideOfAssignment(fieldReference.Syntax)
                    || IsOutArgument(fieldReference.Syntax);

                bool IsOutArgument(SyntaxNode syntaxNode) =>
                    syntaxNode.Parent is ArgumentSyntax argument &&
                    argument.RefOrOutKeyword.IsKind(SyntaxKind.OutKeyword);

                bool IsLeftSideOfAssignment(SyntaxNode syntaxNode) =>
                    syntaxNode.Parent is AssignmentExpressionSyntax assignmentExpression &&
                    assignmentExpression.Left == syntaxNode;
            }

            private SyntaxNodeWithSymbol<SyntaxNode, ISymbol> GetTopmostSyntaxWithTheSameSymbol(SyntaxNode potentialReference)
            {
                var referencedSymbol = semanticModel.GetSymbolInfo(potentialReference).Symbol;

                if (referencedSymbol != null)
                {
                    while (potentialReference.Parent != null
                        && referencedSymbol.Equals(semanticModel.GetSymbolInfo(potentialReference.Parent).Symbol))
                    {
                        potentialReference = potentialReference.Parent;
                    }

                    if (potentialReference.Parent != null &&
                        potentialReference.Parent.IsKind(SyntaxKind.ConditionalAccessExpression))
                    {
                        potentialReference = potentialReference.Parent;
                    }
                }

                return new SyntaxNodeWithSymbol<SyntaxNode, ISymbol>(potentialReference, referencedSymbol);
            }
        }

        private class Lookup<TKey, TElement> : Dictionary<TKey, HashSet<TElement>>
        {
        }
    }
}
