/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.CSharp.Rules
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public sealed class StaticFieldInitializerOrder : SonarDiagnosticAnalyzer
    {
        private const string DiagnosticId = "S3263";
        private const string MessageFormat = "Move this field's initializer into a static constructor.";

        private static readonly DiagnosticDescriptor Rule = DescriptorFactory.Create(DiagnosticId, MessageFormat);

        private static readonly HashSet<SyntaxKind> EnclosingTypes =
            [
                SyntaxKind.ClassDeclaration,
                SyntaxKind.InterfaceDeclaration,
                SyntaxKind.StructDeclaration,
                SyntaxKindEx.RecordDeclaration,
                SyntaxKindEx.RecordStructDeclaration,
            ];

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
                {
                    var fieldDeclaration = (FieldDeclarationSyntax)c.Node;
                    if (!fieldDeclaration.Modifiers.Any(SyntaxKind.StaticKeyword))
                    {
                        return;
                    }
                    var variables = fieldDeclaration.Declaration.Variables.Where(x => x.Initializer != null).ToArray();
                    if (variables.Length == 0)
                    {
                        return;
                    }
                    var containingType = c.Model.GetDeclaredSymbol(variables[0]).ContainingType;
                    var typeDeclaration = fieldDeclaration.FirstAncestorOrSelf<TypeDeclarationSyntax>(x => x.IsAnyKind(EnclosingTypes));

                    foreach (var variable in variables)
                    {
                        if (IdentifierFields(variable, containingType, c.Model)
                                .Select(x => new IdentifierTypeDeclarationMapping(x, GetTypeDeclaration(x)))
                                .Any(x => x.TypeDeclaration is not null && (x.TypeDeclaration != typeDeclaration || x.Field.DeclaringSyntaxReferences.First().Span.Start > variable.SpanStart)))
                        {
                            c.ReportIssue(Rule, variable.Initializer);
                        }
                    }
                },
                SyntaxKind.FieldDeclaration);

        private static IEnumerable<IFieldSymbol> IdentifierFields(VariableDeclaratorSyntax variable, INamedTypeSymbol containingType, SemanticModel semanticModel)
        {
            foreach (var identifier in variable.Initializer.DescendantNodes().OfType<IdentifierNameSyntax>())
            {
                if (containingType.MemberNames.Contains(identifier.Identifier.ValueText)
                    && semanticModel.GetSymbolInfo(identifier).Symbol is IFieldSymbol { IsConst: false, IsStatic: true } field
                    && containingType.Equals(field.ContainingType)
                    && semanticModel.GetEnclosingSymbol(identifier.SpanStart) is IFieldSymbol enclosingSymbol
                    && enclosingSymbol.ContainingType.Equals(field.ContainingType))
                {
                    yield return field;
                }
            }
        }

        private static TypeDeclarationSyntax GetTypeDeclaration(IFieldSymbol field) =>
            field.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax().FirstAncestorOrSelf<TypeDeclarationSyntax>();

        private sealed record IdentifierTypeDeclarationMapping(IFieldSymbol Field, TypeDeclarationSyntax TypeDeclaration);
    }
}
