/*
 * SonarAnalyzer for .NET
 * Copyright (C) SonarSource Sàrl
 * mailto:info AT sonarsource DOT com
 *
 * You can redistribute and/or modify this program under the terms of
 * the Sonar Source-Available License Version 1, as published by SonarSource Sàrl.
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

namespace SonarAnalyzer.Core.Facade;

public interface ILanguageFacade
{
    AssignmentFinder AssignmentFinder { get; }
    StringComparison NameComparison { get; }
    StringComparer NameComparer { get; }
    GeneratedCodeRecognizer GeneratedCodeRecognizer { get; }
    IExpressionNumericConverter ExpressionNumericConverter { get; }

    DiagnosticDescriptor CreateDescriptor(string id, string messageFormat, bool? isEnabledByDefault = null, bool fadeOutCode = false, bool isCompilationEnd = false);

    /// <param name="strict">If true, result derived from field initializers and parameter default values will be omitted. Use it when you need certainty about the value.</param>
    object FindConstantValue(SemanticModel model, SyntaxNode node, bool strict = false);
    IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, IMethodSymbol methodSymbol);
    IMethodParameterLookup MethodParameterLookup(SyntaxNode invocation, SemanticModel model);
    string GetName(SyntaxNode expression);
}

public interface ILanguageFacade<TSyntaxKind> : ILanguageFacade where TSyntaxKind : struct
{
    SyntaxFacade<TSyntaxKind> Syntax { get; }
    ISyntaxKindFacade<TSyntaxKind> SyntaxKind { get; }
    ITrackerFacade<TSyntaxKind> Tracker { get; }
}
