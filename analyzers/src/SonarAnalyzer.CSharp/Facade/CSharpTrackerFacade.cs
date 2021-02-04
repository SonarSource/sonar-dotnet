/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2021 SonarSource SA
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

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using SonarAnalyzer.Common;

namespace SonarAnalyzer.Helpers.Facade
{
    internal sealed class CSharpTrackerFacade : ITrackerFacade<SyntaxKind>
    {
        public BaseTypeTracker<SyntaxKind> BaseType(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) =>
            new CSharpBaseTypeTracker(analyzerConfiguration, rule);

        public ElementAccessTracker<SyntaxKind> ElementAccess(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) =>
            new CSharpElementAccessTracker(analyzerConfiguration, rule);

        public FieldAccessTracker<SyntaxKind> FieldAccess(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) =>
            new CSharpFieldAccessTracker(analyzerConfiguration, rule);

        public InvocationTracker<SyntaxKind> Invocation(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) =>
            new CSharpInvocationTracker(analyzerConfiguration, rule);

        public MethodDeclarationTracker MethodDeclaration(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) =>
            new CSharpMethodDeclarationTracker(analyzerConfiguration, rule);

        public ObjectCreationTracker<SyntaxKind> ObjectCreation(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) =>
            new CSharpObjectCreationTracker(analyzerConfiguration, rule);

        public PropertyAccessTracker<SyntaxKind> PropertyAccess(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) =>
            new CSharpPropertyAccessTracker(analyzerConfiguration, rule);
    }
}
