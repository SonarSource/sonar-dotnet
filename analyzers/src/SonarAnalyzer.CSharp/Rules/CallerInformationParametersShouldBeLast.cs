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
public sealed class CallerInformationParametersShouldBeLast : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3343";
    private const string MessageFormat = "Move '{0}' to the end of the parameter list.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(
            ReportOnViolation,
            SyntaxKind.MethodDeclaration,
            SyntaxKind.ConstructorDeclaration,
            SyntaxKind.DelegateDeclaration,
            SyntaxKindEx.LocalFunctionStatement,
            SyntaxKind.ClassDeclaration,
            SyntaxKindEx.RecordDeclaration,
            SyntaxKindEx.RecordStructDeclaration);

    private static void ReportOnViolation(SonarSyntaxNodeReportingContext context)
    {
        var methodDeclaration = context.Node;
        if (methodDeclaration.ParameterList() is not { Parameters.Count: > 0 } parameterList)
        {
            return;
        }

        if (context.Model.GetDeclaredSymbol(methodDeclaration) is not { } methodSymbol
            || methodSymbol.IsOverride
            || methodSymbol.InterfaceMembers().Any())
        {
            return;
        }

        CheckParameterList(parameterList, context.Model, context);
        CheckPartialConstructorParameterList(methodSymbol, context);
    }

    private static void CheckPartialConstructorParameterList(ISymbol methodSymbol, SonarSyntaxNodeReportingContext context)
    {
        if (methodSymbol is IMethodSymbol { MethodKind: MethodKind.Constructor, PartialDefinitionPart: { } definitionPart })
        {
            foreach (var partialReference in definitionPart.DeclaringSyntaxReferences)
            {
                if (partialReference.GetSyntax() is BaseMethodDeclarationSyntax declaration
                    && declaration.EnsureCorrectSemanticModelOrDefault(context.Model) is { } model)
                {
                    CheckParameterList(declaration.ParameterList, model, context);
                }
            }
        }
    }

    private static void CheckParameterList(ParameterListSyntax parameterList, SemanticModel model, SonarSyntaxNodeReportingContext context)
    {
        ParameterSyntax trailingRegularParameter = null;
        foreach (var parameter in parameterList.Parameters.Reverse())
        {
            if (IsCallerInfoParameter(parameter, model))
            {
                if (trailingRegularParameter is not null
                    && HasIdentifier(parameter)
                    && DeclaresCallerInfoAttribute(parameter, model))
                {
                    context.ReportIssue(Rule, parameter, parameter.Identifier.Text);
                }
            }
            else
            {
                trailingRegularParameter = parameter;
            }
        }
    }

    // Semantic check: the parameter symbol carries a caller-info attribute. For partial methods/constructors,
    // this is true on every declaration of the parameter, because Roslyn merges attributes across partial parts.
    private static bool IsCallerInfoParameter(ParameterSyntax parameter, SemanticModel model) =>
        model.GetDeclaredSymbol(parameter)?.GetAttributes(KnownType.CallerInfoAttributes).Any() == true;

    // Syntactic check: the caller-info attribute is physically written on this parameter's source.
    // Used to anchor the diagnostic on the declaration the user actually wrote, avoiding duplicate reports
    // on a sibling partial declaration that only inherits the attribute via symbol merging.
    private static bool DeclaresCallerInfoAttribute(ParameterSyntax parameter, SemanticModel model) =>
        parameter.AttributeLists.GetAttributes(KnownType.CallerInfoAttributes, model).Any();

    private static bool HasIdentifier(ParameterSyntax parameter) =>
        !string.IsNullOrEmpty(parameter.Identifier.Text);
}
