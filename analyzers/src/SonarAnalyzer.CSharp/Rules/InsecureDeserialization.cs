/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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
public sealed class InsecureDeserialization : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S5766";
    private const string MessageFormat = "Validate data in this deserialization constructor.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var declaration = (TypeDeclarationSyntax)c.Node;
                if (!c.IsRedundantPositionalRecordContext()
                    && HasConstructorsWithParameters(declaration) // If there are no constructors, or if these don't have parameters, there is no validation done and the type is considered safe.
                    && c.Model.GetDeclaredSymbol(declaration) is { } typeSymbol
                    && HasSerializableAttribute(typeSymbol))
                {
                    ReportOnInsecureDeserializations(c, declaration, typeSymbol);
                }
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);

    private static void ReportOnInsecureDeserializations(SonarSyntaxNodeReportingContext context, TypeDeclarationSyntax declaration, ITypeSymbol typeSymbol)
    {
        var implementsISerializable = ImplementsISerializable(typeSymbol);
        var implementsIDeserializationCallback = ImplementsIDeserializationCallback(typeSymbol);

        var walker = new ConstructorDeclarationWalker(context.Model);
        walker.SafeVisit(declaration);

        if (!implementsISerializable && !implementsIDeserializationCallback)
        {
            foreach (var constructor in walker.ConstructorsInfo(x => x.HasConditionalConstructs))
            {
                ReportIssue(context, constructor);
            }
        }

        if (implementsISerializable && !walker.HasDeserializationCtorWithConditionalStatements())
        {
            foreach (var constructor in walker.ConstructorsInfo(x => !x.IsDeserializationConstructor && x.HasConditionalConstructs))
            {
                ReportIssue(context, constructor);
            }
        }

        if (implementsIDeserializationCallback && !OnDeserializationHasConditions(declaration, context.Model))
        {
            foreach (var constructor in walker.ConstructorsInfo(x => x.HasConditionalConstructs))
            {
                ReportIssue(context, constructor);
            }
        }

        static void ReportIssue(SonarSyntaxNodeReportingContext context, ConstructorInfo constructor) =>
            context.ReportIssue(Rule, constructor.ReportLocation());
    }

    private static bool OnDeserializationHasConditions(TypeDeclarationSyntax typeDeclaration, SemanticModel model) =>
        typeDeclaration
            .Members
            .OfType<MethodDeclarationSyntax>()
            .FirstOrDefault(x => IsOnDeserialization(x, model))
            .ContainsConditionalConstructs();

    private static bool IsOnDeserialization(MethodDeclarationSyntax methodDeclaration, SemanticModel model) =>
        methodDeclaration.Identifier.Text == nameof(System.Runtime.Serialization.IDeserializationCallback.OnDeserialization)
        && methodDeclaration.ParameterList.Parameters.Count == 1
        && methodDeclaration.ParameterList.Parameters[0].IsDeclarationKnownType(KnownType.System_Object, model);

    private static bool HasConstructorsWithParameters(TypeDeclarationSyntax typeDeclaration) =>
        typeDeclaration
            .Members
            .OfType<ConstructorDeclarationSyntax>()
            .Any(x => x.ParameterList.Parameters.Count > 0);

    private static bool HasSerializableAttribute(ISymbol symbol) =>
        symbol.HasAttribute(KnownType.System_SerializableAttribute);

    private static bool ImplementsISerializable(ITypeSymbol symbol) =>
        symbol.Implements(KnownType.System_Runtime_Serialization_ISerializable);

    private static bool ImplementsIDeserializationCallback(ITypeSymbol symbol) =>
        symbol.Implements(KnownType.System_Runtime_Serialization_IDeserializationCallback);

    /// <summary>
    /// This walker is responsible to visit all constructor declarations and check if parameters are used in a
    /// conditional structure or not.
    /// </summary>
    private sealed class ConstructorDeclarationWalker : SafeCSharpSyntaxWalker
    {
        private readonly SemanticModel model;
        private readonly List<ConstructorInfo> constructorsInfo = [];

        private bool visitedFirstLevel;

        public ConstructorDeclarationWalker(SemanticModel model) =>
            this.model = model;

        public IEnumerable<ConstructorInfo> ConstructorsInfo(Func<ConstructorInfo, bool> predicate) =>
            constructorsInfo.Where(predicate);

        public bool HasDeserializationCtorWithConditionalStatements() =>
            DeserializationConstructor() is { HasConditionalConstructs: true };

        public override void VisitConstructorDeclaration(ConstructorDeclarationSyntax node)
        {
            var isDeserializationCtor = IsDeserializationConstructor(node);

            var hasConditionalStatements = isDeserializationCtor
                ? node.ContainsConditionalConstructs()
                : HasParametersUsedInConditionalConstructs(node);

            constructorsInfo.Add(new ConstructorInfo(node, hasConditionalStatements, isDeserializationCtor));

            base.VisitConstructorDeclaration(node);
        }

        public override void VisitClassDeclaration(ClassDeclarationSyntax node)
        {
            if (visitedFirstLevel)
            {
                // Skip nested visits. The rule will be triggered for them also.
                return;
            }

            visitedFirstLevel = true;
            base.VisitClassDeclaration(node);
        }

        public override void Visit(SyntaxNode node)
        {
            if (node.Kind() is SyntaxKindEx.RecordDeclaration or SyntaxKindEx.RecordStructDeclaration)
            {
                if (visitedFirstLevel)
                {
                    // Skip nested visits. The rule will be triggered for them also.
                    return;
                }

                visitedFirstLevel = true;
            }

            base.Visit(node);
        }

        private bool HasParametersUsedInConditionalConstructs(BaseMethodDeclarationSyntax declaration)
        {
            var symbols = ConstructorParameterSymbols(declaration, model);

            var conditionalsWalker = new ConditionalsWalker(model, symbols);
            conditionalsWalker.SafeVisit(declaration);

            return conditionalsWalker.HasParametersUsedInConditionalConstructs;
        }

        private ConstructorInfo DeserializationConstructor() =>
            constructorsInfo.SingleOrDefault(x => x.IsDeserializationConstructor);

        private bool IsDeserializationConstructor(BaseMethodDeclarationSyntax declaration) =>
            // A deserialization ctor has the following parameters: (SerializationInfo information, StreamingContext context)
            // See https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.iserializable?view=netcore-3.1#remarks
            declaration.ParameterList.Parameters.Count == 2
            && declaration.ParameterList.Parameters[0].IsDeclarationKnownType(KnownType.System_Runtime_Serialization_SerializationInfo, model)
            && declaration.ParameterList.Parameters[1].IsDeclarationKnownType(KnownType.System_Runtime_Serialization_StreamingContext, model);

        private static ImmutableArray<ISymbol> ConstructorParameterSymbols(BaseMethodDeclarationSyntax node, SemanticModel model) =>
            node.ParameterList.Parameters
                .Select(x => (ISymbol)model.GetDeclaredSymbol(x))
                .ToImmutableArray();
    }

    /// <summary>
    /// This walker is responsible to visit all conditional structures and check if a list of parameters
    /// are used or not.
    /// </summary>
    private sealed class ConditionalsWalker : SafeCSharpSyntaxWalker
    {
        private readonly SemanticModel model;
        private readonly ISet<string> parameterNames;

        public bool HasParametersUsedInConditionalConstructs { get; private set; }

        public ConditionalsWalker(SemanticModel model, ImmutableArray<ISymbol> parameters)
        {
            this.model = model;

            parameterNames = parameters.Select(x => x.Name).ToHashSet();
        }

        public override void VisitIfStatement(IfStatementSyntax node)
        {
            UpdateParameterValidationStatus(node.Condition);

            base.VisitIfStatement(node);
        }

        public override void VisitConditionalExpression(ConditionalExpressionSyntax node)
        {
            UpdateParameterValidationStatus(node.Condition);

            base.VisitConditionalExpression(node);
        }

        public override void VisitSwitchStatement(SwitchStatementSyntax node)
        {
            UpdateParameterValidationStatus(node.Expression);

            base.VisitSwitchStatement(node);
        }

        public override void VisitBinaryExpression(BinaryExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKind.CoalesceExpression))
            {
                UpdateParameterValidationStatus(node.Left);
            }

            base.VisitBinaryExpression(node);
        }

        public override void VisitAssignmentExpression(AssignmentExpressionSyntax node)
        {
            if (node.IsKind(SyntaxKindEx.CoalesceAssignmentExpression))
            {
                UpdateParameterValidationStatus(node.Left);
            }

            base.VisitAssignmentExpression(node);
        }

        public override void Visit(SyntaxNode node)
        {
            if (node.IsKind(SyntaxKindEx.SwitchExpression))
            {
                UpdateParameterValidationStatus(((SwitchExpressionSyntaxWrapper)node).GoverningExpression);
            }

            if (node.IsKind(SyntaxKindEx.SwitchExpressionArm))
            {
                var arm = (SwitchExpressionArmSyntaxWrapper)node;

                if (arm.Pattern.Node is not null)
                {
                    UpdateParameterValidationStatus(arm.Pattern);
                }

                if (arm.WhenClause.Node is not null)
                {
                    UpdateParameterValidationStatus(arm.WhenClause);
                }
            }

            base.Visit(node);
        }

        private void UpdateParameterValidationStatus(SyntaxNode node) =>
            HasParametersUsedInConditionalConstructs |= node
                .DescendantNodesAndSelf()
                .OfType<IdentifierNameSyntax>()
                .Where(x => parameterNames.Contains(x.Identifier.Text))
                .Select(x => model.GetSymbolInfo(x).Symbol)
                .Any(x => x is not null);
    }

    private sealed class ConstructorInfo
    {
        private readonly ConstructorDeclarationSyntax declarationSyntax;

        public bool HasConditionalConstructs { get; }

        public bool IsDeserializationConstructor { get; }

        public ConstructorInfo(
            ConstructorDeclarationSyntax declaration,
            bool hasConditionalConstructs,
            bool isDeserializationConstructor)
        {
            declarationSyntax = declaration;
            HasConditionalConstructs = hasConditionalConstructs;
            IsDeserializationConstructor = isDeserializationConstructor;
        }

        public Location ReportLocation() =>
            declarationSyntax.Identifier.GetLocation();
    }
}
