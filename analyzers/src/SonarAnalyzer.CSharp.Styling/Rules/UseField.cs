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

using static Microsoft.CodeAnalysis.Accessibility;

namespace SonarAnalyzer.CSharp.Styling.Rules;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public sealed class UseField : StylingAnalyzer
{
    public UseField() : base("T0038", "Use field instead of this {0} auto-property.") { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(c =>
            {
                var property = (PropertyDeclarationSyntax)c.Node;
                if (c.ContainingSymbol is IPropertySymbol { DeclaredAccessibility: Private or Protected or ProtectedAndInternal, IsAbstract: false, IsVirtual: false, IsOverride: false }
                    && property.AccessorList is { } accessors
                    && accessors.Accessors.All(x => x.Body is null && x.ExpressionBody is null))
                {
                    c.ReportIssue(Rule, property, property.Modifiers.Where(x => x.Kind() is SyntaxKind.PrivateKeyword or SyntaxKind.ProtectedKeyword).JoinStr(" ", x => x.ValueText));
                }
            }, SyntaxKind.PropertyDeclaration);
}
