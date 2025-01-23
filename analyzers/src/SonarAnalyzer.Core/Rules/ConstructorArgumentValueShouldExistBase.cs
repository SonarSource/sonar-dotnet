/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2025 SonarSource SA
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

namespace SonarAnalyzer.Rules;

public abstract class ConstructorArgumentValueShouldExistBase<TSyntaxKind, TAttribute> : SonarDiagnosticAnalyzer<TSyntaxKind>
    where TSyntaxKind : struct
    where TAttribute : SyntaxNode
{
    private const string DiagnosticId = "S4260";

    protected abstract SyntaxNode GetFirstAttributeArgument(TAttribute attributeSyntax);

    protected override string MessageFormat => "Change this 'ConstructorArgumentAttribute' value to match one of the existing constructors arguments.";

    protected ConstructorArgumentValueShouldExistBase() : base(DiagnosticId) { }

    protected override void Initialize(SonarAnalysisContext context) =>
        context.RegisterNodeAction(Language.GeneratedCodeRecognizer,
            c =>
            {
                var attribute = (TAttribute)c.Node;
                if (Language.Syntax.IsKnownAttributeType(c.Model, c.Node, KnownType.System_Windows_Markup_ConstructorArgumentAttribute)
                    && GetFirstAttributeArgument(attribute) is { } firstAttribute
                    && c.Model.GetConstantValue(Language.Syntax.NodeExpression(firstAttribute)) is { HasValue: true, Value: string constructorParameterName }
                    && c.ContainingSymbol is IPropertySymbol { ContainingType: { } containingType }
                    && !GetConstructorParameterNames(containingType).Contains(constructorParameterName))
                {
                    c.ReportIssue(Rule, firstAttribute.GetLocation());
                }
            }, Language.SyntaxKind.Attribute);

    private static IEnumerable<string> GetConstructorParameterNames(INamedTypeSymbol containingSymbol) =>
        containingSymbol?.Constructors.SelectMany(x => x.Parameters).Select(x => x.Name) ?? [];
}
