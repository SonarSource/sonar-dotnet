/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class DoNotCopyArraysInProperties : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S2365";
    private const string MessageFormat = "Refactor '{0}' into a method, properties should not copy collections.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    private static readonly ImmutableArray<KnownType> CopyingCollectionTypes = ImmutableArray.Create(
        KnownType.System_Collections_Generic_Dictionary_TKey_TValue,
        KnownType.System_Collections_Generic_HashSet_T,
        KnownType.System_Collections_Generic_LinkedList_T,
        KnownType.System_Collections_Generic_List_T,
        KnownType.System_Collections_Generic_Queue_T,
        KnownType.System_Collections_Generic_SortedDictionary_TKey_TValue,
        KnownType.System_Collections_Generic_SortedList_TKey_TValue,
        KnownType.System_Collections_Generic_SortedSet_T,
        KnownType.System_Collections_Generic_Stack_T);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            c =>
            {
                var property = (PropertyDeclarationSyntax)c.Node;
                var body = PropertyBody(property);
                if (body is null)
                {
                    return;
                }

                var walker = new PropertyWalker(c.Model, body is ArrowExpressionClauseSyntax);
                walker.SafeVisit(body);
                foreach (var location in walker.Locations)
                {
                    c.ReportIssue(Rule, location, property.Identifier.Text);
                }
            },
            SyntaxKind.PropertyDeclaration);

    private static SyntaxNode PropertyBody(PropertyDeclarationSyntax property) =>
        property.ExpressionBody
        ?? property.AccessorList
            .Accessors
            .Where(x => x.IsKind(SyntaxKind.GetAccessorDeclaration))
            .Select(x => (SyntaxNode)x.Body ?? x.ExpressionBody)
            .FirstOrDefault();

    private sealed class PropertyWalker : SafeCSharpSyntaxWalker
    {
        private readonly SemanticModel model;
        private readonly List<Location> locations = [];
        private bool inGetterReturn;

        public IEnumerable<Location> Locations => locations;

        public PropertyWalker(SemanticModel model, bool isArrowExpression)
        {
            this.model = model;
            inGetterReturn = isArrowExpression;
        }

        public override void VisitInvocationExpression(InvocationExpressionSyntax node)
        {
            if (inGetterReturn
                && model.GetSymbolInfo(node).Symbol is IMethodSymbol invokedSymbol
                && (invokedSymbol.IsArrayClone()
                    || invokedSymbol.IsEnumerableToList()
                    || invokedSymbol.IsEnumerableToArray()))
            {
                locations.Add(node.Expression.GetLocation());
            }
        }

        public override void VisitObjectCreationExpression(ObjectCreationExpressionSyntax node)
        {
            if (inGetterReturn && IsObjectCreationCopyingCollection(node))
            {
                locations.Add(node.GetLocation());
            }
        }

        public override void Visit(SyntaxNode node)
        {
            if (inGetterReturn
                && node.IsKind(SyntaxKindEx.ImplicitObjectCreationExpression)
                && IsObjectCreationCopyingCollection(node))
            {
                locations.Add(node.GetLocation());
            }
            if (node.IsKind(SyntaxKindEx.LocalFunctionStatement))
            {
                return; // Filter local function
            }
            base.Visit(node);
        }

        public override void VisitReturnStatement(ReturnStatementSyntax node)
        {
            inGetterReturn = true;
            base.VisitReturnStatement(node);
            inGetterReturn = false;
        }

        public override void VisitSimpleLambdaExpression(SimpleLambdaExpressionSyntax node)
        {
            // Filter Lambda
        }

        public override void VisitParenthesizedLambdaExpression(ParenthesizedLambdaExpressionSyntax node)
        {
            // Filter Lambda
        }

        public override void VisitAnonymousMethodExpression(AnonymousMethodExpressionSyntax node)
        {
            // Filter Anonymous Method
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            // Filter lazy initialization
        }

        private bool IsObjectCreationCopyingCollection(SyntaxNode node) =>
            model.GetSymbolInfo(node).Symbol is IMethodSymbol { MethodKind: MethodKind.Constructor, Parameters.Length: > 0 } constructor
            && constructor.ContainingType.OriginalDefinition.IsAny(CopyingCollectionTypes)
            && constructor.Parameters[0] is var firstParameter
            && !firstParameter.Type.Is(KnownType.System_String)
            && firstParameter.Type.DerivesOrImplements(KnownType.System_Collections_IEnumerable);
    }
}
