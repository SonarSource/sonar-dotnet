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
    public sealed class DisposableNotDisposed : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S2930";
        private const string MessageFormat = "Dispose '{0}' when it is no longer needed.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly ImmutableArray<KnownType> TrackedTypes =
            ImmutableArray.Create(
                KnownType.FluentAssertions_Execution_AssertionScope,
                KnownType.System_Drawing_Image,
                KnownType.System_Drawing_Bitmap,
                KnownType.System_IO_FileStream,
                KnownType.System_IO_StreamReader,
                KnownType.System_IO_StreamWriter,
                KnownType.System_Net_WebClient,
                KnownType.System_Net_Sockets_TcpClient,
                KnownType.System_Net_Sockets_UdpClient);

        private static readonly ImmutableArray<KnownType> DisposableTypes = ImmutableArray.Create(KnownType.System_IDisposable, KnownType.System_IAsyncDisposable);

        private static readonly ISet<string> DisposeMethods = new HashSet<string> { "Dispose", "DisposeAsync", "Close" };

        private static readonly ISet<string> FactoryMethods = new HashSet<string>
        {
            "System.IO.File.Create",
            "System.IO.File.Open",
            "System.Drawing.Image.FromFile",
            "System.Drawing.Image.FromStream"
        };

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterSymbolAction(
                c =>
                {
                    var namedType = (INamedTypeSymbol)c.Symbol;
                    if (namedType.ContainingType != null || !namedType.IsClassOrStruct())
                    {
                        return;
                    }

                    var typesDeclarationsAndSemanticModels = namedType.DeclaringSyntaxReferences
                                                                            .Select(x => CreateNodeAndModel(c, x))
                                                                            .ToList();

                    var disposableObjects = new HashSet<NodeAndSymbol>();
                    foreach (var typeDeclarationAndSemanticModel in typesDeclarationsAndSemanticModels)
                    {
                        TrackInitializedLocalsAndPrivateFields(
                            namedType,
                            typeDeclarationAndSemanticModel.Node,
                            typeDeclarationAndSemanticModel.Model,
                            disposableObjects);

                        TrackAssignmentsToLocalsAndPrivateFields(
                            namedType,
                            typeDeclarationAndSemanticModel.Node,
                            typeDeclarationAndSemanticModel.Model,
                            disposableObjects);
                    }

                    if (disposableObjects.Any())
                    {
                        var possiblyDisposed = new HashSet<ISymbol>();
                        foreach (var typeDeclarationAndSemanticModel in typesDeclarationsAndSemanticModels)
                        {
                            ExcludeDisposedAndClosedLocalsAndPrivateFields(
                                typeDeclarationAndSemanticModel.Node,
                                typeDeclarationAndSemanticModel.Model,
                                possiblyDisposed);
                            ExcludeReturnedPassedAndAliasedLocalsAndPrivateFields(
                                typeDeclarationAndSemanticModel.Node,
                                typeDeclarationAndSemanticModel.Model,
                                possiblyDisposed);
                        }

                        foreach (var disposable in disposableObjects.Where(x => !possiblyDisposed.Contains(x.Symbol)))
                        {
                            c.ReportIssue(CreateDiagnostic(Rule, disposable.Node.GetLocation(), disposable.Symbol.Name));
                        }
                    }
                },
                SymbolKind.NamedType);

        private static NodeAndModel<SyntaxNode> CreateNodeAndModel(SonarSymbolReportingContext c, SyntaxReference syntaxReference) =>
            new(c.Compilation.GetSemanticModel(syntaxReference.SyntaxTree), syntaxReference.GetSyntax());

        private static void TrackInitializedLocalsAndPrivateFields(INamedTypeSymbol namedType,
                                                                   SyntaxNode typeDeclaration,
                                                                   SemanticModel semanticModel,
                                                                   ISet<NodeAndSymbol> disposableObjects)
        {
            var descendantNodes = GetDescendantNodes(namedType, typeDeclaration).ToList();
            var localVariableDeclarations = descendantNodes.OfType<LocalDeclarationStatementSyntax>()
                                                           .Where(x => !x.UsingKeyword().IsKind(SyntaxKind.UsingKeyword))
                                                           .Select(x => x.Declaration);

            var fieldVariableDeclarations = descendantNodes.OfType<FieldDeclarationSyntax>()
                                                           .Where(x => !x.Modifiers.Any() || x.Modifiers.Any(SyntaxKind.PrivateKeyword))
                                                           .Select(x => x.Declaration);

            foreach (var declaration in localVariableDeclarations.Concat(fieldVariableDeclarations))
            {
                var trackedVariables = declaration.Variables.Where(x => x.Initializer != null && IsInstantiation(x.Initializer.Value, semanticModel));
                foreach (var variableNode in trackedVariables)
                {
                    disposableObjects.Add(new NodeAndSymbol(variableNode, semanticModel.GetDeclaredSymbol(variableNode)));
                }
            }
        }

        private static void TrackAssignmentsToLocalsAndPrivateFields(INamedTypeSymbol namedType,
                                                                     SyntaxNode typeDeclaration,
                                                                     SemanticModel semanticModel,
                                                                     ISet<NodeAndSymbol> disposableObjects)
        {
            var simpleAssignments = GetDescendantNodes(namedType, typeDeclaration).Where(n => n.IsKind(SyntaxKind.SimpleAssignmentExpression))
                                                                                  .Cast<AssignmentExpressionSyntax>();

            foreach (var simpleAssignment in simpleAssignments)
            {
                if (!IsNodeInsideUsingStatement(simpleAssignment)
                    && IsInstantiation(simpleAssignment.Right, semanticModel)
                    && semanticModel.GetSymbolInfo(simpleAssignment.Left).Symbol is { } referencedSymbol
                    && IsLocalOrPrivateField(referencedSymbol))
                {
                    disposableObjects.Add(new NodeAndSymbol(simpleAssignment, referencedSymbol));
                }
            }
        }

        private static bool IsNodeInsideUsingStatement(SyntaxNode node)
        {
            var ancestors = node.AncestorsAndSelf().ToArray();

            var isPartOfUsingStatement = ancestors
                .OfType<UsingStatementSyntax>()
                .Any(x => (x.Expression is not null && x.Expression.DescendantNodesAndSelf().Contains(node))
                       || (x.Declaration is not null && x.Declaration.DescendantNodesAndSelf().Contains(node)));

            var isPartOfUsingDeclaration = ancestors
                .OfType<LocalDeclarationStatementSyntax>()
                .Any(x => x.UsingKeyword() != default);

            return isPartOfUsingStatement || isPartOfUsingDeclaration;
        }

        private static IEnumerable<SyntaxNode> GetDescendantNodes(INamedTypeSymbol namedType, SyntaxNode typeDeclaration) =>
            namedType.IsTopLevelProgram()
                ? typeDeclaration.ChildNodes().OfType<GlobalStatementSyntax>().Select(x => x.ChildNodes().First())
                : typeDeclaration.DescendantNodes();

        private static bool IsLocalOrPrivateField(ISymbol symbol) =>
            symbol.Kind == SymbolKind.Local
            || (symbol.Kind == SymbolKind.Field && symbol.DeclaredAccessibility == Accessibility.Private);

        private static void ExcludeDisposedAndClosedLocalsAndPrivateFields(SyntaxNode typeDeclaration, SemanticModel semanticModel, ISet<ISymbol> possiblyDisposed)
        {
            var invocationsAndConditionalAccesses = typeDeclaration.DescendantNodes().Where(n => n.IsAnyKind(SyntaxKind.InvocationExpression, SyntaxKind.ConditionalAccessExpression));
            foreach (var invocationOrConditionalAccess in invocationsAndConditionalAccesses)
            {
                SimpleNameSyntax name;
                ExpressionSyntax expression;
                if (invocationOrConditionalAccess is InvocationExpressionSyntax invocation)
                {
                    var memberAccessNode = invocation.Expression as MemberAccessExpressionSyntax;
                    name = memberAccessNode?.Name;
                    expression = memberAccessNode?.Expression;
                }
                else if (invocationOrConditionalAccess is ConditionalAccessExpressionSyntax conditionalAccess)
                {
                    name = ((conditionalAccess.WhenNotNull as InvocationExpressionSyntax)?.Expression as MemberBindingExpressionSyntax)?.Name;
                    expression = conditionalAccess.Expression;
                }
                else
                {
                    throw new NotSupportedException("Syntax node should be either an invocation or a conditional access expression");
                }

                if (name != null
                    && (DisposeMethods.Contains(name.Identifier.Text) || IsNodeInsideUsingStatement(expression))
                    && semanticModel.GetSymbolInfo(expression).Symbol is { } referencedSymbol
                    && IsLocalOrPrivateField(referencedSymbol))
                {
                    possiblyDisposed.Add(referencedSymbol);
                }
            }
        }

        private static void ExcludeReturnedPassedAndAliasedLocalsAndPrivateFields(SyntaxNode typeDeclaration, SemanticModel semanticModel, ISet<ISymbol> possiblyDisposed)
        {
            var identifiersAndSimpleMemberAccesses = typeDeclaration
                .DescendantNodes()
                .Where(n => n.IsKind(SyntaxKind.IdentifierName) || n.IsKind(SyntaxKind.SimpleMemberAccessExpression));

            foreach (var identifierOrSimpleMemberAccess in identifiersAndSimpleMemberAccesses)
            {
                ExpressionSyntax expression;
                if (identifierOrSimpleMemberAccess.IsKind(SyntaxKind.IdentifierName))
                {
                    expression = (IdentifierNameSyntax)identifierOrSimpleMemberAccess;
                }
                else if (identifierOrSimpleMemberAccess.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                {
                    var memberAccess = (MemberAccessExpressionSyntax)identifierOrSimpleMemberAccess;
                    if (memberAccess.Expression.IsKind(SyntaxKind.ThisExpression))
                    {
                        expression = memberAccess;
                    }
                    else
                    {
                        continue;
                    }
                }
                else
                {
                    throw new NotSupportedException("Syntax node should be either an identifier or a simple member access expression");
                }

                if (IsStandaloneExpression(expression)
                    && semanticModel.GetSymbolInfo(identifierOrSimpleMemberAccess).Symbol is { } referencedSymbol
                    && IsLocalOrPrivateField(referencedSymbol))
                {
                    possiblyDisposed.Add(referencedSymbol);
                }
            }
        }

        private static bool IsStandaloneExpression(ExpressionSyntax expression) =>
            !(expression.Parent is ExpressionSyntax)
            || (expression.Parent is AssignmentExpressionSyntax parentAsAssignment && ReferenceEquals(expression, parentAsAssignment.Right));

        private static bool IsInstantiation(ExpressionSyntax expression, SemanticModel semanticModel) =>
            IsNewTrackedTypeObjectCreation(expression, semanticModel)
            || IsDisposableRefStructCreation(expression, semanticModel)
            || IsFactoryMethodInvocation(expression, semanticModel);

        private static bool IsNewTrackedTypeObjectCreation(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression)
            && semanticModel.GetTypeInfo(expression).Type is var type
            && type.IsAny(TrackedTypes)
            && semanticModel.GetSymbolInfo(expression).Symbol is IMethodSymbol constructor
            && !constructor.Parameters.Any(x => x.Type.ImplementsAny(DisposableTypes));

        private static bool IsDisposableRefStructCreation(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression.IsAnyKind(SyntaxKind.ObjectCreationExpression, SyntaxKindEx.ImplicitObjectCreationExpression)
            && semanticModel.GetTypeInfo(expression).Type is var type
            && type.IsStruct()
            && type.IsRefLikeType()
            && type.GetMembers().OfType<IMethodSymbol>().Any(x => x.Name == "Dispose");

        private static bool IsFactoryMethodInvocation(ExpressionSyntax expression, SemanticModel semanticModel) =>
            expression is InvocationExpressionSyntax invocation
            && semanticModel.GetSymbolInfo(invocation).Symbol is IMethodSymbol methodSymbol
            && FactoryMethods.Contains(methodSymbol.ContainingType.ToDisplayString() + "." + methodSymbol.Name);
    }
}
