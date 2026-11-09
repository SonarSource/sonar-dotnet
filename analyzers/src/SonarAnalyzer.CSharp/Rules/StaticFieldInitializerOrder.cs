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
public sealed class StaticFieldInitializerOrder : SonarDiagnosticAnalyzer
{
    private const string DiagnosticId = "S3263";
    private const string MessageFormat = "Move this field's initializer into a static constructor.";

    private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                if (!fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                {
                    return;
                }
                var variables = fieldDeclaration.Declaration.Variables.Where(x => x.Initializer is not null).ToArray();
                if (variables.Length == 0)
                {
                    return;
                }
                var containingType = c.Model.GetDeclaredSymbol(variables[0]).ContainingType;
                var typeDeclaration = fieldDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>();

                foreach (var variable in variables)
                {
                    if (IdentifierFields(variable, containingType, c.Model)
                            .Select(x => new IdentifierTypeDeclarationMapping(x, ContainingTypeDeclaration(x)))
                            .Any(x => x.TypeDeclaration is not null && (x.TypeDeclaration != typeDeclaration || x.Field.DeclaringSyntaxReferences.First().Span.Start > variable.SpanStart)))
                    {
                        c.ReportIssue(Rule, variable.Initializer);
                    }
                }
            },
            SyntaxKind.FieldDeclaration);

    private static IEnumerable<IFieldSymbol> IdentifierFields(VariableDeclaratorSyntax variable, INamedTypeSymbol containingType, SemanticModel model)
    {
        foreach (var identifier in variable.Initializer.DescendantNodes().OfType<IdentifierNameSyntax>())
        {
            if (containingType.MemberNames.Contains(identifier.Identifier.ValueText)
                && model.GetSymbolInfo(identifier).Symbol is IFieldSymbol { IsConst: false, IsStatic: true } field
                && containingType.Equals(field.ContainingType)
                && model.GetEnclosingSymbol(identifier.SpanStart) is IFieldSymbol enclosingSymbol
                && enclosingSymbol.ContainingType.Equals(field.ContainingType))
            {
                yield return field;
            }
        }
    }

    private static TypeDeclarationSyntax ContainingTypeDeclaration(IFieldSymbol field) =>
        field.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().FirstAncestorOrSelf<TypeDeclarationSyntax>();

    private sealed record IdentifierTypeDeclarationMapping(IFieldSymbol Field, TypeDeclarationSyntax TypeDeclaration);
}
