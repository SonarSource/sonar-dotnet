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

using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using ObjectCreationCondition = SonarAnalyzer.Helpers.TrackingCondition<SonarAnalyzer.Helpers.ObjectCreationContext>;

namespace SonarAnalyzer.Helpers
{
    public abstract class ObjectCreationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, ObjectCreationContext>
        where TSyntaxKind : struct
    {
        internal abstract ObjectCreationCondition ArgumentAtIndexIsConst(int index);
        internal abstract object ConstArgumentForParameter(ObjectCreationContext context, string parameterName);

        protected ObjectCreationTracker(IAnalyzerConfiguration analyzerConfiguration, DiagnosticDescriptor rule) : base(analyzerConfiguration, rule) { }

        internal ObjectCreationCondition ArgumentIsBoolConstant(string parameterName, bool expectedValue) =>
            context => ConstArgumentForParameter(context, parameterName) is bool boolValue && boolValue == expectedValue;

        internal ObjectCreationCondition ArgumentAtIndexIs(int index, KnownType type) =>
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.Parameters.Length > index
                       && context.InvokedConstructorSymbol.Value.Parameters[index].Type.Is(type);

        internal ObjectCreationCondition WhenDerivesOrImplementsAny(params KnownType[] types) =>
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.IsConstructor()
                       && context.InvokedConstructorSymbol.Value.ContainingType.DerivesOrImplementsAny(types.ToImmutableArray());

        internal ObjectCreationCondition MatchConstructor(params KnownType[] types) =>
            // We cannot do a syntax check first because a type name can be aliased with
            // a using Alias = Fully.Qualified.Name and we will generate false negative
            // for new Alias()
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.IsConstructor()
                       && context.InvokedConstructorSymbol.Value.ContainingType.IsAny(types);

        internal ObjectCreationCondition WhenDerivesFrom(KnownType baseType) =>
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.IsConstructor()
                       && context.InvokedConstructorSymbol.Value.ContainingType.DerivesFrom(baseType);

        internal ObjectCreationCondition WhenImplements(KnownType baseType) =>
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.IsConstructor()
                       && context.InvokedConstructorSymbol.Value.ContainingType.Implements(baseType);

        protected override SyntaxBaseContext CreateContext(SyntaxNode expression, SemanticModel semanticModel) =>
            new ObjectCreationContext(expression, semanticModel);
    }
}
