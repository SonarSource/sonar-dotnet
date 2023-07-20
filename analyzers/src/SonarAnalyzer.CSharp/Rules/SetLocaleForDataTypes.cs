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
    public sealed class SetLocaleForDataTypes : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S4057";
        private const string MessageFormat = "Set the locale for this '{0}'.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
        private static readonly ImmutableArray<KnownType> CheckedTypes = ImmutableArray.Create(
            KnownType.System_Data_DataTable,
            KnownType.System_Data_DataSet);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);
        protected override bool EnableConcurrentExecution => false;

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterCompilationStartAction(
                compilationStartContext =>
                {
                    var symbolsWhereTypeIsCreated = new Dictionary<ISymbol, SyntaxNode>();
                    var symbolsWhereLocaleIsSet = new HashSet<ISymbol>();

                    compilationStartContext.RegisterNodeAction(
                        c => ProcessObjectCreations(c, symbolsWhereTypeIsCreated),
                        SyntaxKind.ObjectCreationExpression,
                        SyntaxKindEx.ImplicitObjectCreationExpression);
                    compilationStartContext.RegisterNodeAction(c => ProcessSimpleAssignments(c, symbolsWhereLocaleIsSet), SyntaxKind.SimpleAssignmentExpression);
                    compilationStartContext.RegisterCompilationEndAction(c => ProcessCollectedSymbols(c, symbolsWhereTypeIsCreated, symbolsWhereLocaleIsSet));
                });

        private static void ProcessObjectCreations(SonarSyntaxNodeReportingContext c, IDictionary<ISymbol, SyntaxNode> symbolsWhereTypeIsCreated)
        {
            if (GetSymbolFromConstructorInvocation(c.Node, c.SemanticModel) is ITypeSymbol objectType
                && objectType.IsAny(CheckedTypes)
                && GetAssignmentTargetVariable(c.Node) is { } variableSyntax)
            {
                if (DeclarationExpressionSyntaxWrapper.IsInstance(variableSyntax))
                {
                    variableSyntax = ((DeclarationExpressionSyntaxWrapper)variableSyntax).Designation;
                }

                var variableSymbol = variableSyntax is IdentifierNameSyntax
                    ? c.SemanticModel.GetSymbolInfo(variableSyntax).Symbol
                    : c.SemanticModel.GetDeclaredSymbol(variableSyntax);

                if (variableSymbol != null && !symbolsWhereTypeIsCreated.ContainsKey(variableSymbol))
                {
                    symbolsWhereTypeIsCreated.Add(variableSymbol, c.Node);
                }
            }
        }

        private static void ProcessSimpleAssignments(SonarSyntaxNodeReportingContext c, ISet<ISymbol> symbolsWhereLocaleIsSet)
        {
            var assignmentExpression = (AssignmentExpressionSyntax)c.Node;
            var variableSymbols = assignmentExpression.AssignmentTargets()
                .Where(x => c.SemanticModel.GetSymbolInfo(x).Symbol is IPropertySymbol propertySymbol
                            && propertySymbol.Name == "Locale"
                            && propertySymbol.ContainingType.IsAny(CheckedTypes))
                .Select(x => GetAccessedVariable(x, c.SemanticModel))
                .WhereNotNull();
            symbolsWhereLocaleIsSet.UnionWith(variableSymbols);
        }

        private static void ProcessCollectedSymbols(SonarCompilationReportingContext c, IDictionary<ISymbol, SyntaxNode> symbolsWhereTypeIsCreated, ISet<ISymbol> symbolsWhereLocaleIsSet)
        {
            foreach (var invalidCreation in symbolsWhereTypeIsCreated.Where(x => !symbolsWhereLocaleIsSet.Contains(x.Key)))
            {
                if (invalidCreation.Key.GetSymbolType() is { } type)
                {
                    c.ReportIssue(CreateDiagnostic(Rule, invalidCreation.Value.GetLocation(), type.Name));
                }
            }
        }

        private static ISymbol GetSymbolFromConstructorInvocation(SyntaxNode constructorCall, SemanticModel semanticModel) =>
            constructorCall is ObjectCreationExpressionSyntax objectCreation
                ? semanticModel.GetSymbolInfo(objectCreation.Type).Symbol
                : semanticModel.GetSymbolInfo(constructorCall).Symbol?.ContainingType;

        private static SyntaxNode GetAssignmentTargetVariable(SyntaxNode objectCreation) =>
            objectCreation.GetFirstNonParenthesizedParent() switch
            {
                AssignmentExpressionSyntax assignment => assignment.Left,
                EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator } => declarator,
                ArgumentSyntax argument => argument.FindAssignmentComplement(),
                _ => null,
            };

        private static ISymbol GetAccessedVariable(SyntaxNode node, SemanticModel model) =>
            node.RemoveParentheses() switch
            {
                IdentifierNameSyntax
                {
                    Parent: AssignmentExpressionSyntax
                    {
                        Parent: InitializerExpressionSyntax
                        {
                            Parent: { RawKind: (int)SyntaxKind.ObjectCreationExpression or (int)SyntaxKindEx.ImplicitObjectCreationExpression } objectCreation
                        }
                    }
                } => GetAssignmentTargetSymbol(objectCreation, model), // Locale is assigned in an object initializer. Find the target of the object creation.
                MemberAccessExpressionSyntax memberAccessExpression => model.GetSymbolInfo(memberAccessExpression.Expression).Symbol,
                _ => null,
            };

        private static ISymbol GetAssignmentTargetSymbol(SyntaxNode objectCreation, SemanticModel model)
        {
            var leftSideOfParentAssignment = objectCreation.GetFirstNonParenthesizedParent() switch
            {
                // var dt = new DataTable { Locale = l }
                EqualsValueClauseSyntax { Parent: VariableDeclaratorSyntax declarator } => declarator,
                // dt = new DataTable { Locale = l }
                AssignmentExpressionSyntax assignment => assignment.Left,
                // var (dt, _) = (new DataTable { Locale = l }, 42)
                ArgumentSyntax argumentSyntax => argumentSyntax.FindAssignmentComplement(),
                _ => null,
            };

            return leftSideOfParentAssignment switch
            {
                null => null,
                IdentifierNameSyntax => model.GetSymbolInfo(leftSideOfParentAssignment).Symbol,
                _ when DeclarationExpressionSyntaxWrapper.IsInstance(leftSideOfParentAssignment) => model.GetSymbolInfo(leftSideOfParentAssignment).Symbol,
                _ => model.GetDeclaredSymbol(leftSideOfParentAssignment),
            };
        }
    }
}
