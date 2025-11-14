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

namespace SonarAnalyzer.Core.Trackers;

public class InvocationContext : SyntaxBaseContext
{
    public string MethodName { get; }
    public Lazy<IMethodSymbol> MethodSymbol { get; }

    public InvocationContext(SonarSyntaxNodeReportingContext context, string methodName) : this(context.Node, methodName, context.Model) { }

    public InvocationContext(SyntaxNode node, string methodName, SemanticModel model) : base(node, model)
    {
        MethodName = methodName;
        MethodSymbol = new Lazy<IMethodSymbol>(() => Model.GetSymbolInfo(Node).Symbol as IMethodSymbol);
    }
}
