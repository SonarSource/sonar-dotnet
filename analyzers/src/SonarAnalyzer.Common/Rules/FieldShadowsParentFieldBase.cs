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
    public abstract class FieldShadowsParentFieldBase<TSyntaxKind, TVariableDeclaratorSyntax> : SonarDiagnosticAnalyzer
        where TSyntaxKind : struct
        where TVariableDeclaratorSyntax : SyntaxNode
    {
        protected const string S2387DiagnosticId = "S2387";
        private const string S2387MessageFormat = "'{0}' is the name of a field in '{1}'.";

        protected const string S4025DiagnosticId = "S4025";
        private const string S4025MessageFormat = "Rename this field; it may be confused with '{0}' in '{1}'.";

        private readonly DiagnosticDescriptor s2387;
        private readonly DiagnosticDescriptor s4025;

        protected abstract ILanguageFacade<TSyntaxKind> Language { get; }

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(s2387, s4025);

        protected FieldShadowsParentFieldBase()
        {
            s2387 = Language.CreateDescriptor(S2387DiagnosticId, S2387MessageFormat);
            s4025 = Language.CreateDescriptor(S4025DiagnosticId, S4025MessageFormat);
        }

        protected IEnumerable<Diagnostic> CheckFields(SemanticModel semanticModel, TVariableDeclaratorSyntax variableDeclarator)
        {
            if (semanticModel.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol fieldSymbol)
            {
                var fieldName = fieldSymbol.Name;
                var fieldNameUpper = fieldSymbol.Name.ToUpperInvariant();
                foreach (var baseType in fieldSymbol.ContainingType.BaseType.GetSelfAndBaseTypes())
                {
                    var similarFields = baseType.GetMembers().OfType<IFieldSymbol>().Where(IsMatch).ToList();
                    if (similarFields.Any(field => field.Name == fieldName))
                    {
                        yield return CreateDiagnostic(s2387, Language.Syntax.NodeIdentifier(variableDeclarator).Value.GetLocation(), fieldName, baseType.Name);
                    }
                    else if (similarFields.Any())
                    {
                        yield return CreateDiagnostic(s4025, Language.Syntax.NodeIdentifier(variableDeclarator).Value.GetLocation(), similarFields.First().Name, baseType.Name);
                    }
                }

                bool IsMatch(IFieldSymbol field) =>
                    field.DeclaredAccessibility != Accessibility.Private
                    && !field.IsStatic
                    && field.Name.ToUpperInvariant() == fieldNameUpper;
            }
        }
    }
}
