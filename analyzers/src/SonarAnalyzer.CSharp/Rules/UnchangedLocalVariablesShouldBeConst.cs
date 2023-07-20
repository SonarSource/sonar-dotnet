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

namespace SonarAnalyzer.Rules.CSharp;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UnchangedLocalVariablesShouldBeConst : SonarDiagnosticAnalyzer
{
    internal const string DiagnosticId = "S3353";
    private const string MessageFormat = "Add the 'const' modifier to '{0}'{1}."; // {1} is a placeholder for optional MessageFormatVarHint
    private const string MessageFormatVarHint = ", and replace 'var' with '{0}'";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    private enum DeclarationType
    {
        CannotBeConst,
        Value,
        Reference,
        String
    }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var localDeclaration = (LocalDeclarationStatementSyntax)c.Node;
                if (localDeclaration.Modifiers.Any(SyntaxKind.ConstKeyword))
                {
                    return;
                }

                var declaredType = FindDeclarationType(localDeclaration, c.SemanticModel);
                if (declaredType == DeclarationType.CannotBeConst)
                {
                    return;
                }

                localDeclaration.Declaration?.Variables
                    .Where(v => v is { Identifier: { } }
                        // constant string interpolation is only valid in C# 10 and above
                        && (c.SemanticModel.Compilation.IsAtLeastLanguageVersion(LanguageVersionEx.CSharp10) || !ContainsInterpolation(v))
                        && IsInitializedWithCompatibleConstant(v, c.SemanticModel, declaredType)
                        && !HasMutableUsagesInMethod(c.SemanticModel, v))
                    .ToList()
                    .ForEach(x => Report(c, x));
            },
            SyntaxKind.LocalDeclarationStatement);

    private static DeclarationType FindDeclarationType(LocalDeclarationStatementSyntax localDeclaration, SemanticModel semanticModel)
    {
        var declaredTypeSyntax = localDeclaration.Declaration?.Type;
        if (declaredTypeSyntax == null)
        {
            return DeclarationType.CannotBeConst;
        }

        var declaredType = semanticModel.GetTypeInfo(declaredTypeSyntax).Type;
        if (declaredType == null)
        {
            return DeclarationType.CannotBeConst;
        }
        else if (declaredType.Is(KnownType.System_String))
        {
            return DeclarationType.String;
        }
        else if (declaredType.OriginalDefinition?.DerivesFrom(KnownType.System_Nullable_T) ?? false)
        {
            // Defining nullable as const raises error CS0283.
            return DeclarationType.CannotBeConst;
        }
        else if (declaredType.IsStruct() && declaredType.SpecialType == SpecialType.None && declaredType.GetMembers("op_Implicit").Any(x => !x.IsImplicitlyDeclared))
        {
            // Struct with explicitly declared "implicit operator"
            return DeclarationType.CannotBeConst;
        }
        else
        {
            return declaredType.IsValueType ? DeclarationType.Value : DeclarationType.Reference;
        }
    }

    private static bool IsInitializedWithCompatibleConstant(VariableDeclaratorSyntax variableDeclarator, SemanticModel semanticModel, DeclarationType declarationType) =>
        variableDeclarator is { Initializer.Value: { } initializer }
            && semanticModel.GetConstantValue(initializer) switch
            {
                { HasValue: false } => false,
                { Value: string } => declarationType == DeclarationType.String,
                { Value: ValueType } => declarationType == DeclarationType.Value,
                _ => declarationType is DeclarationType.Reference or DeclarationType.String,
            };

    private static bool HasMutableUsagesInMethod(SemanticModel semanticModel, VariableDeclaratorSyntax variable)
    {
        var parentSyntax = variable.Ancestors().FirstOrDefault(IsMethodLike);
        if (parentSyntax == null)
        {
            return false;
        }
        else if (parentSyntax is GlobalStatementSyntax)
        {
            // If the variable is declared in a top level statement we should search inside the compilation unit.
            parentSyntax = parentSyntax.Parent;
        }

        var variableSymbol = semanticModel.GetDeclaredSymbol(variable);
        return variableSymbol != null
            && parentSyntax.DescendantNodes()
                .OfType<IdentifierNameSyntax>()
                .Where(x => x.GetName().Equals(variableSymbol.Name) && variableSymbol.Equals(semanticModel.GetSymbolInfo(x).Symbol))
                .Any(x => IsMutatingUse(semanticModel, x));

        static bool IsMethodLike(SyntaxNode arg) =>
            arg is BaseMethodDeclarationSyntax
                or AccessorDeclarationSyntax
                or LambdaExpressionSyntax
                or AnonymousFunctionExpressionSyntax
                or GlobalStatementSyntax
            || LocalFunctionStatementSyntaxWrapper.IsInstance(arg);
    }

    private static bool IsMutatingUse(SemanticModel semanticModel, IdentifierNameSyntax identifier) =>
        identifier.Parent switch
        {
            AssignmentExpressionSyntax { Left: { } left } => identifier.Equals(left),
            ArgumentSyntax argumentSyntax => argumentSyntax.IsInTupleAssignmentTarget() || !argumentSyntax.RefOrOutKeyword.IsKind(SyntaxKind.None),
            PostfixUnaryExpressionSyntax => true,
            PrefixUnaryExpressionSyntax => true,
            _ => IsUsedAsLambdaExpression(semanticModel, identifier),
        };

    private static bool IsUsedAsLambdaExpression(SemanticModel semanticModel, IdentifierNameSyntax identifier)
    {
        if (identifier.FirstAncestorOrSelf<LambdaExpressionSyntax>().GetSelfOrTopParenthesizedExpression() is { } lambda)
        {
            if (lambda.Parent is ArgumentSyntax argument
                && argument.FirstAncestorOrSelf<InvocationExpressionSyntax>() is { } invocation)
            {
                var lookup = new CSharpMethodParameterLookup(invocation, semanticModel);
                return lookup.TryGetSymbol(argument, out var parameter) && parameter.IsType(KnownType.System_Linq_Expressions_Expression_T);
            }
            else if (lambda.Parent is AssignmentExpressionSyntax assignment)
            {
                // Lambda cannot be on the left side, we don't need to check it
                return assignment.Left.IsKnownType(KnownType.System_Linq_Expressions_Expression_T, semanticModel);
            }
        }
        return false;
    }

    private static bool ContainsInterpolation(VariableDeclaratorSyntax declaratorSyntax) =>
        declaratorSyntax is { Initializer.Value: { } initializer }
            && initializer.DescendantNodesAndSelf().Any(x => x.IsKind(SyntaxKind.Interpolation));

    private static void Report(SonarSyntaxNodeReportingContext c, VariableDeclaratorSyntax declaratorSyntax) =>
        c.ReportIssue(CreateDiagnostic(Rule,
            declaratorSyntax.Identifier.GetLocation(),
            declaratorSyntax.Identifier.ValueText,
            AddionalMessageHints(c.SemanticModel, declaratorSyntax)));

    private static string AddionalMessageHints(SemanticModel semanticModel, VariableDeclaratorSyntax declaratorSyntax) =>
        declaratorSyntax is { Parent: VariableDeclarationSyntax { Type: { IsVar: true } typeSyntax } }
            ? string.Format(MessageFormatVarHint, semanticModel.GetTypeInfo(typeSyntax).Type.ToMinimalDisplayString(semanticModel, typeSyntax.SpanStart))
            : string.Empty;
}
