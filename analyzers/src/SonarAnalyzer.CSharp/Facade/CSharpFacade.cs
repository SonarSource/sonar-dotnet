/*
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

using SonarAnalyzer.CSharp.Core.Facade.Implementation;
using SonarAnalyzer.CSharp.Core.Trackers;
using SonarAnalyzer.Helpers.Facade;

namespace SonarAnalyzer.CSharp.Core.Facade;

public sealed class CSharpFacade : ILanguageFacade<SyntaxKind>
{
    private static readonly Lazy<CSharpFacade> Singleton = new(() => new CSharpFacade());
    private static readonly Lazy<AssignmentFinder> AssignmentFinderLazy = new(() => new CSharpAssignmentFinder());
    private static readonly Lazy<IExpressionNumericConverter> ExpressionNumericConverterLazy = new(() => new CSharpExpressionNumericConverter());
    private static readonly Lazy<SyntaxFacade<SyntaxKind>> SyntaxLazy = new(() => new CSharpSyntaxFacade());
    private static readonly Lazy<ISyntaxKindFacade<SyntaxKind>> SyntaxKindLazy = new(() => new CSharpSyntaxKindFacade());
    private static readonly Lazy<ITrackerFacade<SyntaxKind>> TrackerLazy = new(() => new CSharpTrackerFacade());

    public AssignmentFinder AssignmentFinder => AssignmentFinderLazy.Value;
    public StringComparison NameComparison => StringComparison.Ordinal;
    public StringComparer NameComparer => StringComparer.Ordinal;
    public GeneratedCodeRecognizer GeneratedCodeRecognizer => CSharpGeneratedCodeRecognizer.Instance;
    public IExpressionNumericConverter ExpressionNumericConverter => ExpressionNumericConverterLazy.Value;
    public SyntaxFacade<SyntaxKind> Syntax => SyntaxLazy.Value;
    public ISyntaxKindFacade<SyntaxKind> SyntaxKind => SyntaxKindLazy.Value;
    public ITrackerFacade<SyntaxKind> Tracker => TrackerLazy.Value;

    public static CSharpFacade Instance => Singleton.Value;

    private CSharpFacade() { }

    public DiagnosticDescriptor CreateDescriptor(string id, string messageFormat, bool? isEnabledByDefault = null, bool fadeOutCode = false) =>
        DescriptorFactory.Create(id, messageFormat, isEnabledByDefault, fadeOutCode);

    public object FindConstantValue(SemanticModel model, SyntaxNode node) =>
        node.FindConstantValue(model);

    public IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, IMethodSymbol methodSymbol) =>
        invocation switch
        {
            null => null,
            AttributeSyntax x => new CSharpAttributeParameterLookup(x, methodSymbol),
            _ => new CSharpMethodParameterLookup(invocation.ArgumentList(), methodSymbol),
        };

    public IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, SemanticModel semanticModel) =>
        invocation?.ArgumentList() is { } argumentList
            ? new CSharpMethodParameterLookup(argumentList, semanticModel)
            : null;

    public string GetName(SyntaxNode expression) =>
        expression.GetName();
}
