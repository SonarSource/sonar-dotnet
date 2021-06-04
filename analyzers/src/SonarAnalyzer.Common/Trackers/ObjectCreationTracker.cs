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
using Microsoft.CodeAnalysis.Diagnostics;

namespace SonarAnalyzer.Helpers.Trackers
{
    public abstract class ObjectCreationTracker<TSyntaxKind> : SyntaxTrackerBase<TSyntaxKind, ObjectCreationContext>
        where TSyntaxKind : struct
    {
        protected override TSyntaxKind[] TrackedSyntaxKinds => Language.SyntaxKind.ObjectCreationExpressions;

        internal abstract Condition ArgumentAtIndexIsConst(int index);
        internal abstract object ConstArgumentForParameter(ObjectCreationContext context, string parameterName);

        public Condition ExceptWhen(Condition condition) =>
            value => !condition(value);

        public Condition And(Condition condition1, Condition condition2) =>
            value => condition1(value) && condition2(value);

        public Condition Or(Condition condition1, Condition condition2) =>
            value => condition1(value) || condition2(value);

        public Condition Or(Condition condition1, Condition condition2, Condition condition3) =>
            value => condition1(value) || condition2(value) || condition3(value);

        internal Condition ArgumentIsBoolConstant(string parameterName, bool expectedValue) =>
            context => ConstArgumentForParameter(context, parameterName) is bool boolValue && boolValue == expectedValue;

        internal Condition ArgumentAtIndexIs(int index, KnownType type) =>
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.Parameters.Length > index
                       && context.InvokedConstructorSymbol.Value.Parameters[index].Type.Is(type);

        internal Condition WhenDerivesOrImplementsAny(params KnownType[] types) =>
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.IsConstructor()
                       && context.InvokedConstructorSymbol.Value.ContainingType.DerivesOrImplementsAny(types.ToImmutableArray());

        internal Condition MatchConstructor(params KnownType[] types) =>
            // We cannot do a syntax check first because a type name can be aliased with
            // a using Alias = Fully.Qualified.Name and we will generate false negative
            // for new Alias()
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.IsConstructor()
                       && context.InvokedConstructorSymbol.Value.ContainingType.IsAny(types);

        internal Condition WhenDerivesFrom(KnownType baseType) =>
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.IsConstructor()
                       && context.InvokedConstructorSymbol.Value.ContainingType.DerivesFrom(baseType);

        internal Condition WhenImplements(KnownType baseType) =>
            context => context.InvokedConstructorSymbol.Value != null
                       && context.InvokedConstructorSymbol.Value.IsConstructor()
                       && context.InvokedConstructorSymbol.Value.ContainingType.Implements(baseType);

        protected override ObjectCreationContext CreateContext(SyntaxNodeAnalysisContext context) =>
            new ObjectCreationContext(context);
    }
}
