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

namespace SonarAnalyzer.Rules
{
    public abstract class MutableFieldsShouldNotBe : SonarDiagnosticAnalyzer
    {
        private static readonly ImmutableArray<KnownType> MutableBaseTypes =
            ImmutableArray.Create(
                KnownType.System_Collections_Generic_ICollection_T,
                KnownType.System_Array);

        private static readonly ImmutableArray<KnownType> ImmutableBaseTypes =
            ImmutableArray.Create(
                KnownType.System_Collections_ObjectModel_ReadOnlyCollection_T,
                KnownType.System_Collections_ObjectModel_ReadOnlyDictionary_TKey_TValue,
                KnownType.System_Collections_Immutable_IImmutableArray_T,
                KnownType.System_Collections_Immutable_IImmutableDictionary_TKey_TValue,
                KnownType.System_Collections_Immutable_IImmutableList_T,
                KnownType.System_Collections_Immutable_IImmutableSet_T,
                KnownType.System_Collections_Immutable_IImmutableStack_T,
                KnownType.System_Collections_Immutable_IImmutableQueue_T);

        private readonly DiagnosticDescriptor rule;

        protected abstract ISet<SyntaxKind> InvalidModifiers { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(rule);

        protected MutableFieldsShouldNotBe(string diagnosticId, string messageFormat) =>
             rule = DescriptorFactory.Create(diagnosticId, messageFormat);

        protected sealed override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                if (c.IsRedundantPositionalRecordContext())
                {
                    return;
                }
                var typeDeclaration = (TypeDeclarationSyntax)c.Node;
                var fieldDeclarations = typeDeclaration.Members.OfType<FieldDeclarationSyntax>();
                var assignmentsImmutability = FieldAssignmentImmutability(typeDeclaration, fieldDeclarations, c.SemanticModel);

                foreach (var fieldDeclaration in fieldDeclarations)
                {
                    if (HasAllInvalidModifiers(fieldDeclaration)
                        && fieldDeclaration.Declaration.Variables.Count > 0
                        && c.SemanticModel.GetDeclaredSymbol(fieldDeclaration.Declaration.Variables[0]) is IFieldSymbol { Type: not null } fieldSymbol
                        && fieldSymbol.GetEffectiveAccessibility() == Accessibility.Public
                        && !IsImmutableOrValidMutableType(fieldSymbol.Type)
                        // The field seems to be violating the rule but we should exclude the cases where the field is read-only
                        // and all initializations to this field are immutable
                        && CollectInvalidFieldVariables(fieldDeclaration, assignmentsImmutability, c.SemanticModel).ToList() is { Count: > 0 } incorrectFieldVariables)
                    {
                        var pluralizeSuffix = incorrectFieldVariables.Count > 1 ? "s" : string.Empty;
                        c.ReportIssue(CreateDiagnostic(rule, fieldDeclaration.Declaration.Type.GetLocation(), pluralizeSuffix, incorrectFieldVariables.ToSentence(quoteWords: true)));
                    }
                }
            },
            SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKindEx.RecordClassDeclaration, SyntaxKindEx.RecordStructDeclaration);

        private bool HasAllInvalidModifiers(FieldDeclarationSyntax fieldDeclaration) =>
            fieldDeclaration.Modifiers.Count(m => InvalidModifiers.Contains(m.Kind())) == InvalidModifiers.Count;

        private static Dictionary<string, bool?> FieldAssignmentImmutability(TypeDeclarationSyntax typeDeclaration, IEnumerable<FieldDeclarationSyntax> fieldDeclarations, SemanticModel semanticModel)
        {
            var variableNames = fieldDeclarations.SelectMany(x => x.Declaration.Variables)
                .Select(x => x.Identifier.ValueText)
                .ToHashSet();

            var ctorAssignments = typeDeclaration.Members.OfType<ConstructorDeclarationSyntax>()
                .SelectMany(x => x.DescendantNodes())
                .OfType<AssignmentExpressionSyntax>();

            var variableToImmutability = variableNames.ToDictionary(x => x, x => (bool?)null);

            foreach (var assignment in ctorAssignments)
            {
                if (assignment.Left is not IdentifierNameSyntax identifierName
                    || !variableNames.Contains(identifierName.Identifier.ValueText)
                    || variableToImmutability[identifierName.Identifier.ValueText] == false)
                {
                    continue;
                }

                variableToImmutability[identifierName.Identifier.ValueText] = IsImmutableOrValidMutableType(semanticModel.GetTypeInfo(assignment.Right).Type, assignment.Right);
            }

            return variableToImmutability;
        }

        private static IEnumerable<string> CollectInvalidFieldVariables(FieldDeclarationSyntax fieldDeclaration, Dictionary<string, bool?> assignmentsInCtors, SemanticModel semanticModel) =>
            fieldDeclaration.Modifiers.Any(SyntaxKind.ReadOnlyKeyword)
            ? CollectReadonlyInvalidFieldVariables(fieldDeclaration, assignmentsInCtors, semanticModel)
            : fieldDeclaration.Declaration.Variables.Select(x => x.Identifier.ValueText);

        private static IEnumerable<string> CollectReadonlyInvalidFieldVariables(FieldDeclarationSyntax fieldDeclaration, Dictionary<string, bool?> assignmentsInCtors, SemanticModel semanticModel)
        {
            foreach (var variable in fieldDeclaration.Declaration.Variables)
            {
                var onlyInitializedWithImmutablesInCtor = assignmentsInCtors[variable.Identifier.ValueText];

                if (onlyInitializedWithImmutablesInCtor == false)
                {
                    yield return variable.Identifier.ValueText;
                }

                if (variable.Initializer == null
                    || semanticModel.GetSymbolInfo(variable.Initializer.Value).Symbol is not IMethodSymbol methodSymbol)
                {
                    continue;
                }

                var typeSymbol = methodSymbol.MethodKind == MethodKind.Constructor
                    ? methodSymbol.ContainingType
                    : methodSymbol.ReturnType;

                if (!IsImmutableOrValidMutableType(typeSymbol, variable.Initializer.Value))
                {
                    yield return variable.Identifier.ValueText;
                }
            }
        }

        private static bool IsImmutableOrValidMutableType(ITypeSymbol typeSymbol, ExpressionSyntax value = null)
        {
            if (value.IsNullLiteral())
            {
                return true;
            }

            if (typeSymbol is INamedTypeSymbol namedTypeSymbol)
            {
                typeSymbol = namedTypeSymbol.ConstructedFrom;
            }

            return !typeSymbol.DerivesOrImplementsAny(MutableBaseTypes)
                   || typeSymbol.DerivesOrImplementsAny(ImmutableBaseTypes);
        }
    }
}
