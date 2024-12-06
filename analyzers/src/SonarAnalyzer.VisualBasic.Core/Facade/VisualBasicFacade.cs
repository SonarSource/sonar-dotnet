/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2014-2024 SonarSource SA
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

using SonarAnalyzer.Core.Trackers;
using SonarAnalyzer.VisualBasic.Core.Facade.Implementation;
using SonarAnalyzer.VisualBasic.Core.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Facade;

public sealed class VisualBasicFacade : ILanguageFacade<SyntaxKind>
{
    private static readonly Lazy<VisualBasicFacade> Singleton = new(() => new VisualBasicFacade());
    private static readonly Lazy<AssignmentFinder> AssignmentFinderLazy = new(() => new VisualBasicAssignmentFinder());
    private static readonly Lazy<IExpressionNumericConverter> ExpressionNumericConverterLazy = new(() => new VisualBasicExpressionNumericConverter());
    private static readonly Lazy<SyntaxFacade<SyntaxKind>> SyntaxLazy = new(() => new VisualBasicSyntaxFacade());
    private static readonly Lazy<ISyntaxKindFacade<SyntaxKind>> SyntaxKindLazy = new(() => new VisualBasicSyntaxKindFacade());
    private static readonly Lazy<ITrackerFacade<SyntaxKind>> TrackerLazy = new(() => new VisualBasicTrackerFacade());

    public static VisualBasicFacade Instance => Singleton.Value;

    public AssignmentFinder AssignmentFinder => AssignmentFinderLazy.Value;
    public StringComparison NameComparison => StringComparison.OrdinalIgnoreCase;
    public StringComparer NameComparer => StringComparer.OrdinalIgnoreCase;
    public GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;
    public IExpressionNumericConverter ExpressionNumericConverter => ExpressionNumericConverterLazy.Value;
    public SyntaxFacade<SyntaxKind> Syntax => SyntaxLazy.Value;
    public ISyntaxKindFacade<SyntaxKind> SyntaxKind => SyntaxKindLazy.Value;
    public ITrackerFacade<SyntaxKind> Tracker => TrackerLazy.Value;

    private VisualBasicFacade() { }

    public DiagnosticDescriptor CreateDescriptor(string id, string messageFormat, bool? isEnabledByDefault = null, bool fadeOutCode = false) =>
        DescriptorFactory.Create(id, messageFormat, isEnabledByDefault, fadeOutCode);

    public object FindConstantValue(SemanticModel model, SyntaxNode node) =>
        node.FindConstantValue(model);

    public IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, IMethodSymbol methodSymbol) =>
        invocation switch
        {
            null => null,
            AttributeSyntax x => new VisualBasicAttributeParameterLookup(x.ArgumentList.Arguments, methodSymbol),
            IdentifierNameSyntax
            {
                Parent: NameColonEqualsSyntax { Parent: SimpleArgumentSyntax { IsNamed: true, Parent.Parent: AttributeSyntax attribute } }
            } => new VisualBasicAttributeParameterLookup(attribute.ArgumentList.Arguments, methodSymbol),
            _ => new VisualBasicMethodParameterLookup(invocation.ArgumentList(), methodSymbol),
        };

    public IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, SemanticModel model) =>
        invocation?.ArgumentList() is { } argumentList
            ? new VisualBasicMethodParameterLookup(argumentList, model)
            : null;

    public string GetName(SyntaxNode expression) =>
        expression.GetName();
}
