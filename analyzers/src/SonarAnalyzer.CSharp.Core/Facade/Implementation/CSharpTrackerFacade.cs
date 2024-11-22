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
using SonarAnalyzer.CSharp.Core.Trackers;

namespace SonarAnalyzer.CSharp.Core.Facade.Implementation;

internal sealed class CSharpTrackerFacade : ITrackerFacade<SyntaxKind>
{
    public ArgumentTracker<SyntaxKind> Argument { get; } = new CSharpArgumentTracker();
    public BaseTypeTracker<SyntaxKind> BaseType { get; } = new CSharpBaseTypeTracker();
    public ElementAccessTracker<SyntaxKind> ElementAccess { get; } = new CSharpElementAccessTracker();
    public FieldAccessTracker<SyntaxKind> FieldAccess { get; } = new CSharpFieldAccessTracker();
    public InvocationTracker<SyntaxKind> Invocation { get; } = new CSharpInvocationTracker();
    public MethodDeclarationTracker<SyntaxKind> MethodDeclaration { get; } = new CSharpMethodDeclarationTracker();
    public ObjectCreationTracker<SyntaxKind> ObjectCreation { get; } = new CSharpObjectCreationTracker();
    public PropertyAccessTracker<SyntaxKind> PropertyAccess { get; } = new CSharpPropertyAccessTracker();
}
