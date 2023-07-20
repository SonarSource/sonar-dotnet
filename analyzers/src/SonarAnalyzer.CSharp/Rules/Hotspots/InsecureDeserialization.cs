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
    public sealed class InsecureDeserialization : HotspotDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S5766";
        private const string MessageFormat = "Make sure not performing data validation after deserialization is safe here.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

        public InsecureDeserialization() : this(AnalyzerConfiguration.Hotspot) { }
        public InsecureDeserialization(IAnalyzerConfiguration analyzerConfiguration) : base(analyzerConfiguration) { }

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var declaration = (TypeDeclarationSyntax)c.Node;
                if (!c.IsRedundantPositionalRecordContext()
                    && IsEnabled(c.Options)
                    && HasConstructorsWithParameters(declaration) // If there are no constructors, or if these don't have parameters, there is no validation done and the type is considered safe.
                    && c.SemanticModel.GetDeclaredSymbol(declaration) is { } typeSymbol
                    && HasSerializableAttribute(typeSymbol))
                {
                    ReportOnInsecureDeserializations(c, declaration, typeSymbol);
                }
            },
            SyntaxKind.ClassDeclaration,
            SyntaxKindEx.RecordClassDeclaration,
            SyntaxKindEx.RecordStructDeclaration,
            SyntaxKind.StructDeclaration);

        private static void ReportOnInsecureDeserializations(SonarSyntaxNodeReportingContext context, TypeDeclarationSyntax declaration, ITypeSymbol typeSymbol)
        {
            var implementsISerializable = ImplementsISerializable(typeSymbol);
            var implementsIDeserializationCallback = ImplementsIDeserializationCallback(typeSymbol);

            var walker = new ConstructorDeclarationWalker(context.SemanticModel);
            walker.SafeVisit(declaration);

            if (!implementsISerializable && !implementsIDeserializationCallback)
            {
                foreach (var constructor in walker.GetConstructorsInfo(x => x.HasConditionalConstructs))
                {
                    ReportIssue(context, constructor);
                }
            }

            if (implementsISerializable && !walker.HasDeserializationCtorWithConditionalStatements())
            {
                foreach (var constructor in walker.GetConstructorsInfo(x => !x.IsDeserializationConstructor && x.HasConditionalConstructs))
                {
                    ReportIssue(context, constructor);
                }
            }

            if (implementsIDeserializationCallback && !OnDeserializationHasConditions(declaration, context.SemanticModel))
            {
                foreach (var constructor in walker.GetConstructorsInfo(x => x.HasConditionalConstructs))
                {
                    ReportIssue(context, constructor);
                }
            }

            static void ReportIssue(SonarSyntaxNodeReportingContext context, ConstructorInfo constructor) =>
                context.ReportIssue(CreateDiagnostic(Rule, constructor.GetReportLocation()));
        }

        private static bool OnDeserializationHasConditions(TypeDeclarationSyntax typeDeclaration, SemanticModel semanticModel) =>
            typeDeclaration
                .Members
                .OfType<MethodDeclarationSyntax>()
                .FirstOrDefault(methodDeclaration => IsOnDeserialization(methodDeclaration, semanticModel))
                .ContainsConditionalConstructs();

        private static bool IsOnDeserialization(MethodDeclarationSyntax methodDeclaration, SemanticModel semanticModel) =>
            methodDeclaration.Identifier.Text == nameof(System.Runtime.Serialization.IDeserializationCallback.OnDeserialization)
            && methodDeclaration.ParameterList.Parameters.Count == 1
            && methodDeclaration.ParameterList.Parameters[0].IsDeclarationKnownType(KnownType.System_Object, semanticModel);

        private static bool HasConstructorsWithParameters(TypeDeclarationSyntax typeDeclaration) =>
            typeDeclaration
                .Members
                .OfType<ConstructorDeclarationSyntax>()
                .Any(constructorDeclaration => constructorDeclaration.ParameterList.Parameters.Count > 0);

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
            private readonly SemanticModel semanticModel;
            private readonly List<ConstructorInfo> constructorsInfo = new();

            private bool visitedFirstLevel;

            public ConstructorDeclarationWalker(SemanticModel semanticModel)
            {
                this.semanticModel = semanticModel;
            }

            public IEnumerable<ConstructorInfo> GetConstructorsInfo(Func<ConstructorInfo, bool> predicate) =>
                constructorsInfo.Where(predicate);

            public bool HasDeserializationCtorWithConditionalStatements() =>
                GetDeserializationConstructor() is { HasConditionalConstructs: true };

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
                if (node.IsAnyKind(SyntaxKindEx.RecordClassDeclaration, SyntaxKindEx.RecordStructDeclaration))
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
                var symbols = GetConstructorParameterSymbols(declaration, semanticModel);

                var conditionalsWalker = new ConditionalsWalker(semanticModel, symbols);
                conditionalsWalker.SafeVisit(declaration);

                return conditionalsWalker.HasParametersUsedInConditionalConstructs;
            }

            private ConstructorInfo GetDeserializationConstructor() =>
                constructorsInfo.SingleOrDefault(info => info.IsDeserializationConstructor);

            private bool IsDeserializationConstructor(BaseMethodDeclarationSyntax declaration) =>
                // A deserialization ctor has the following parameters: (SerializationInfo information, StreamingContext context)
                // See https://docs.microsoft.com/en-us/dotnet/api/system.runtime.serialization.iserializable?view=netcore-3.1#remarks
                declaration.ParameterList.Parameters.Count == 2
                && declaration.ParameterList.Parameters[0].IsDeclarationKnownType(KnownType.System_Runtime_Serialization_SerializationInfo, semanticModel)
                && declaration.ParameterList.Parameters[1].IsDeclarationKnownType(KnownType.System_Runtime_Serialization_StreamingContext, semanticModel);

            private static ImmutableArray<ISymbol> GetConstructorParameterSymbols(BaseMethodDeclarationSyntax node, SemanticModel semanticModel) =>
                node.ParameterList.Parameters
                    .Select(syntax => (ISymbol)semanticModel.GetDeclaredSymbol(syntax))
                    .ToImmutableArray();
        }

        /// <summary>
        /// This walker is responsible to visit all conditional structures and check if a list of parameters
        /// are used or not.
        /// </summary>
        private sealed class ConditionalsWalker : SafeCSharpSyntaxWalker
        {
            private readonly SemanticModel semanticModel;
            private readonly ISet<string> parameterNames;

            public ConditionalsWalker(SemanticModel semanticModel, ImmutableArray<ISymbol> parameters)
            {
                this.semanticModel = semanticModel;

                parameterNames = parameters.Select(parameter => parameter.Name).ToHashSet();
            }

            public bool HasParametersUsedInConditionalConstructs { get; private set; }

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

                    if (arm.Pattern.SyntaxNode != null)
                    {
                        UpdateParameterValidationStatus(arm.Pattern);
                    }

                    if (arm.WhenClause.SyntaxNode != null)
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
                    .Where(identifier => parameterNames.Contains(identifier.Identifier.Text))
                    .Select(identifier => semanticModel.GetSymbolInfo(identifier).Symbol)
                    .Any(symbol => symbol != null);
        }

        private sealed class ConstructorInfo
        {
            private readonly ConstructorDeclarationSyntax declarationSyntax;

            public ConstructorInfo(ConstructorDeclarationSyntax declaration,
                bool hasConditionalConstructs,
                bool isDeserializationConstructor)
            {
                declarationSyntax = declaration;
                HasConditionalConstructs = hasConditionalConstructs;
                IsDeserializationConstructor = isDeserializationConstructor;
            }

            public bool HasConditionalConstructs { get; }

            public bool IsDeserializationConstructor { get; }

            public Location GetReportLocation() =>
                declarationSyntax.Identifier.GetLocation();
        }
    }
}
