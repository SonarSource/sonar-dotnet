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
    public sealed class PointersShouldBePrivate : SonarDiagnosticAnalyzer
    {
        internal const string DiagnosticId = "S4000";
        private const string MessageFormat = "Make '{0}' 'private' or 'protected readonly'.";

        private static readonly DiagnosticDescriptor Rule =
            DescriptorFactory.Create(DiagnosticId, MessageFormat);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics { get; } = ImmutableArray.Create(Rule);

        protected override void Initialize(SonarAnalysisContext context) =>
            context.RegisterNodeAction(c =>
            {
                var fieldDeclaration = (FieldDeclarationSyntax)c.Node;

                if (fieldDeclaration.Declaration == null)
                {
                    return;
                }

                foreach (var variable in fieldDeclaration.Declaration.Variables)
                {
                    if (SymbolIfPointerType(fieldDeclaration.Declaration, variable, c.SemanticModel) is { } variableSymbol
                        && variableSymbol.GetEffectiveAccessibility() is var accessibility
                        && accessibility != Accessibility.Private
                        && accessibility != Accessibility.Internal
                        && !variableSymbol.IsReadOnly)
                    {
                        c.ReportIssue(CreateDiagnostic(Rule, variable.GetLocation(), variableSymbol.Name));
                    }
                }
            },
            SyntaxKind.FieldDeclaration);

        private static IFieldSymbol SymbolIfPointerType(VariableDeclarationSyntax variableDeclaration, VariableDeclaratorSyntax variableDeclarator, SemanticModel semanticModel)
        {
            if (variableDeclaration.Type.IsKind(SyntaxKind.PointerType)
                || IsUnmanagedFunctionPointer(variableDeclaration))
            {
                return (IFieldSymbol)semanticModel.GetDeclaredSymbol(variableDeclarator);
            }
            else
            {
                return IsPointerStructure(variableDeclaration)
                       && ((IFieldSymbol)semanticModel.GetDeclaredSymbol(variableDeclarator)) is { } variableSymbol
                       && variableSymbol.Type.IsAny(KnownType.PointerTypes)
                    ? variableSymbol
                    : null;
            }
        }

        private static bool IsPointerStructure(VariableDeclarationSyntax variableDeclaration) =>
            variableDeclaration.Type is IdentifierNameSyntax identifierName
                ? IsNameOfPointerStruct(identifierName.Identifier.ValueText)
                : variableDeclaration.Type is QualifiedNameSyntax qualifiedName
                  && qualifiedName.Right is IdentifierNameSyntax identifierNameSyntax
                  && IsNameOfPointerStruct(identifierNameSyntax.Identifier.ValueText);

        private static bool IsNameOfPointerStruct(string typeName) =>
            typeName.Equals("IntPtr") || typeName.Equals("UIntPtr");

        private static bool IsUnmanagedFunctionPointer(VariableDeclarationSyntax variableDeclaration) =>
            variableDeclaration.Type.IsKind(SyntaxKindEx.FunctionPointerType)
            && (FunctionPointerTypeSyntaxWrapper)variableDeclaration.Type is var functionPointerType
            && functionPointerType.CallingConvention.SyntaxNode != null
            && !functionPointerType.CallingConvention.ManagedOrUnmanagedKeyword.IsKind(SyntaxKindEx.ManagedKeyword);
    }
}
