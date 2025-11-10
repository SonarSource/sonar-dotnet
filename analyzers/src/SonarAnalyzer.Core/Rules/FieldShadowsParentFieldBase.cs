/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 * This program is free software; you can redistribute it and/or
 * modify it under the terms of the Sonar Source-Available License Version 1, as published by SonarSource SA.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.
 * See the Sonar Source-Available License for more details.
 *
 * You should have received a copy of the Sonar Source-Available License
 * along with this program; if not, see https://sonarsource.com/license/ssal/
 */

namespace SonarAnalyzer.Core.Rules;

[Obsolete("This rule has been deprecated since 9.32")]
public abstract class FieldShadowsParentFieldBase<TSyntaxKind, TVariableDeclaratorSyntax> : SonarDiagnosticAnalyzer
    where TSyntaxKind : struct
    where TVariableDeclaratorSyntax : SyntaxNode
{
    private const string S2387DiagnosticId = "S2387";
    private const string S2387MessageFormat = "'{0}' is the name of a field in '{1}'.";
    private const string S4025DiagnosticId = "S4025";
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

    protected IEnumerable<Diagnostic> CheckFields(SemanticModel model, TVariableDeclaratorSyntax variableDeclarator)
    {
        if (model.GetDeclaredSymbol(variableDeclarator) is IFieldSymbol fieldSymbol)
        {
            var fieldName = fieldSymbol.Name;
            foreach (var baseType in fieldSymbol.ContainingType.BaseType.GetSelfAndBaseTypes())
            {
                var similarFields = baseType.GetMembers().OfType<IFieldSymbol>().Where(IsMatch).ToList();
                if (similarFields.Any(x => x.Name == fieldName))
                {
                    yield return Diagnostic.Create(s2387, Language.Syntax.NodeIdentifier(variableDeclarator).Value.GetLocation(), fieldName, baseType.Name);
                }
                else if (similarFields.Any())
                {
                    yield return Diagnostic.Create(s4025, Language.Syntax.NodeIdentifier(variableDeclarator).Value.GetLocation(), similarFields.First().Name, baseType.Name);
                }
            }

            bool IsMatch(IFieldSymbol field) =>
                field.DeclaredAccessibility != Accessibility.Private
                && !field.IsStatic
                && field.Name.Equals(fieldName, StringComparison.OrdinalIgnoreCase);
        }
    }
}
