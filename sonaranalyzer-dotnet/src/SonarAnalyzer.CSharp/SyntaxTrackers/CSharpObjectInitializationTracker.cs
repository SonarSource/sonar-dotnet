/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2020 SonarSource SA
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
using Microsoft.CodeAnalysis.CSharp.Syntax;
using SonarAnalyzer.Helpers;

namespace SonarAnalyzer.SyntaxTrackers
{
    /// <summary>
    /// Tests if the provided <paramref name="constantValue"/> is equal to an allowed constant (literal) value.
    /// </summary>
    internal delegate bool ConstantValuePredicate(object constantValue);

    /// <summary>
    /// Returns true if the object represented by a <paramref name="symbol"/> should be allowed.
    /// </summary>
    internal delegate bool SymbolValuePredicate(ISymbol symbol);

    /// <summary>
    /// Verifies the initialization of an object, whether one or more properties have been correctly set when the object was initialized.
    /// </summary>
    /// A correct initialization could consist of:
    /// - EITHER invoking the constructor with specific parameters
    /// - OR invoking the constructor and then setting some specific properties on the created object
    public class CSharpObjectInitializationTracker
    {
        private readonly ConstantValuePredicate constantValuePredicate;

        private readonly SymbolValuePredicate objectPredicate;

        private readonly ImmutableArray<KnownType> trackedTypes;

        internal bool IsAllowedConstantValue(object constantValue) => constantValuePredicate(constantValue);

        internal bool IsAllowedObject(ISymbol symbol) => objectPredicate(symbol);

        internal bool IsTrackedType(ExpressionSyntax expression, SemanticModel semanticModel) =>
            semanticModel.GetTypeInfo(expression).Type.IsAny(trackedTypes);

        internal CSharpObjectInitializationTracker(ConstantValuePredicate isAllowedConstantValue, ImmutableArray<KnownType> trackedTypes)
            : this (isAllowedConstantValue, trackedTypes, isAllowedObject: s => false)
        {
        }

        internal CSharpObjectInitializationTracker(ConstantValuePredicate isAllowedConstantValue, ImmutableArray<KnownType> trackedTypes, SymbolValuePredicate isAllowedObject)
        {
            this.constantValuePredicate = isAllowedConstantValue;
            this.objectPredicate = isAllowedObject;
            this.trackedTypes = trackedTypes;
        }
    }
}
