﻿/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2024 SonarSource SA
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

using SonarAnalyzer.Helpers.Facade;

namespace SonarAnalyzer.Helpers;

internal sealed class VisualBasicFacade : ILanguageFacade<SyntaxKind>
{
    private static readonly Lazy<VisualBasicFacade> Singleton = new(() => new VisualBasicFacade());
    private static readonly Lazy<AssignmentFinder> AssignmentFinderLazy = new(() => new VisualBasicAssignmentFinder());
    private static readonly Lazy<IExpressionNumericConverter> ExpressionNumericConverterLazy = new(() => new VisualBasicExpressionNumericConverter());
    private static readonly Lazy<SyntaxFacade<SyntaxKind>> SyntaxLazy = new(() => new VisualBasicSyntaxFacade());
    private static readonly Lazy<ISyntaxKindFacade<SyntaxKind>> SyntaxKindLazy = new(() => new VisualBasicSyntaxKindFacade());
    private static readonly Lazy<ITrackerFacade<SyntaxKind>> TrackerLazy = new(() => new VisualBasicTrackerFacade());

    public AssignmentFinder AssignmentFinder => AssignmentFinderLazy.Value;
    public StringComparison NameComparison => StringComparison.OrdinalIgnoreCase;
    public StringComparer NameComparer => StringComparer.OrdinalIgnoreCase;
    public GeneratedCodeRecognizer GeneratedCodeRecognizer => VisualBasicGeneratedCodeRecognizer.Instance;
    public IExpressionNumericConverter ExpressionNumericConverter => ExpressionNumericConverterLazy.Value;
    public SyntaxFacade<SyntaxKind> Syntax => SyntaxLazy.Value;
    public ISyntaxKindFacade<SyntaxKind> SyntaxKind => SyntaxKindLazy.Value;
    public ITrackerFacade<SyntaxKind> Tracker => TrackerLazy.Value;

    public static VisualBasicFacade Instance => Singleton.Value;

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

    public IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, SemanticModel semanticModel) =>
        invocation?.ArgumentList() is { } argumentList
            ? new VisualBasicMethodParameterLookup(argumentList, semanticModel)
            : null;

    public string GetName(SyntaxNode expression) =>
        expression.GetName();
}
