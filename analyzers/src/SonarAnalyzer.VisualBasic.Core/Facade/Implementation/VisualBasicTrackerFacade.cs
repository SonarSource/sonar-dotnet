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
using SonarAnalyzer.VisualBasic.Core.Trackers;

namespace SonarAnalyzer.VisualBasic.Core.Facade.Implementation;

internal sealed class VisualBasicTrackerFacade : ITrackerFacade<SyntaxKind>
{
    public ArgumentTracker<SyntaxKind> Argument => new VisualBasicArgumentTracker();
    public BaseTypeTracker<SyntaxKind> BaseType { get; } = new VisualBasicBaseTypeTracker();
    public ElementAccessTracker<SyntaxKind> ElementAccess { get; } = new VisualBasicElementAccessTracker();
    public FieldAccessTracker<SyntaxKind> FieldAccess { get; } = new VisualBasicFieldAccessTracker();
    public InvocationTracker<SyntaxKind> Invocation { get; } = new VisualBasicInvocationTracker();
    public MethodDeclarationTracker<SyntaxKind> MethodDeclaration { get; } = new VisualBasicMethodDeclarationTracker();
    public ObjectCreationTracker<SyntaxKind> ObjectCreation { get; } = new VisualBasicObjectCreationTracker();
    public PropertyAccessTracker<SyntaxKind> PropertyAccess { get; } = new VisualBasicPropertyAccessTracker();
}
