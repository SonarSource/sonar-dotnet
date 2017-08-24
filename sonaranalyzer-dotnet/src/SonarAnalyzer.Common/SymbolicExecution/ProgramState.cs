/*
 * SonarAnalyzer for .NET
 * Copyright (C) 2015-2017 SonarSource SA
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

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using SonarAnalyzer.Common;
using SonarAnalyzer.SymbolicExecution.Constraints;
using SonarAnalyzer.SymbolicExecution.Relationships;
using SonarAnalyzer.SymbolicExecution.SymbolicValues;

namespace SonarAnalyzer.SymbolicExecution
{
    public sealed class ProgramState : IEquatable<ProgramState>
    {
        internal ImmutableDictionary<ISymbol, SymbolicValue> Values { get; }
        internal ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> Constraints { get; }
        internal ImmutableDictionary<ProgramPoint, int> ProgramPointVisitCounts { get; }
        internal ImmutableStack<SymbolicValue> ExpressionStack { get; }
        internal ImmutableHashSet<BinaryRelationship> Relationships { get; }

        private static ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> InitialConstraints =>
            new Dictionary<SymbolicValue, SymbolicValueConstraints>
            {
                { SymbolicValue.True,  SymbolicValueConstraints.Create(BoolConstraint.True) },
                { SymbolicValue.False, SymbolicValueConstraints.Create(BoolConstraint.False) },
                { SymbolicValue.Null,  SymbolicValueConstraints.Create(ObjectConstraint.Null) },
                { SymbolicValue.This,  SymbolicValueConstraints.Create(ObjectConstraint.NotNull) },
                { SymbolicValue.Base,  SymbolicValueConstraints.Create(ObjectConstraint.NotNull) }
            }.ToImmutableDictionary();

        private static readonly ISet<SymbolicValue> ProtectedSymbolicValues = ImmutableHashSet.Create(
            SymbolicValue.True,
            SymbolicValue.False,
            SymbolicValue.Null,
            SymbolicValue.This,
            SymbolicValue.Base);

        private static readonly ISet<SymbolicValue> DistinguishedReferences = ImmutableHashSet.Create(
            SymbolicValue.This,
            SymbolicValue.Base);

        internal ProgramState()
            : this(ImmutableDictionary<ISymbol, SymbolicValue>.Empty,
                  InitialConstraints,
                  ImmutableDictionary<ProgramPoint, int>.Empty,
                  ImmutableStack<SymbolicValue>.Empty,
                  ImmutableHashSet<BinaryRelationship>.Empty)
        {
        }

        internal ProgramState TrySetRelationship(BinaryRelationship relationship)
        {
            // Only add new relationships, and ones that are on SV's that belong to a local symbol
            if (Relationships.Contains(relationship) ||
                !IsRelationshipOnLocalValues(relationship))
            {
                return this;
            }

            var relationships = GetAllRelationshipsWith(relationship);
            if (relationships == null)
            {
                return null;
            }

            return new ProgramState(
                Values,
                Constraints,
                ProgramPointVisitCounts,
                ExpressionStack,
                relationships);
        }

        private ImmutableHashSet<BinaryRelationship> GetAllRelationshipsWith(BinaryRelationship relationship)
        {
            var allRelationships = new HashSet<BinaryRelationship>(Relationships);
            var newRelationshipsToProcess = new Queue<BinaryRelationship>(new[] { relationship });
            var newRelationships = new List<BinaryRelationship>(new[] { relationship });

            while (newRelationshipsToProcess.Any())
            {
                var newRelationship = newRelationshipsToProcess.Dequeue();

                if (allRelationships.Contains(newRelationship))
                {
                    continue;
                }

                if (newRelationship.IsContradicting(allRelationships))
                {
                    return null;
                }

                foreach (var transitive in newRelationship.GetTransitiveRelationships(allRelationships))
                {
                    newRelationshipsToProcess.Enqueue(transitive);
                    newRelationships.Add(transitive);
                }

                allRelationships.Add(newRelationship);
            }

            RemoveRedundantRelationships(allRelationships);

            return allRelationships.ToImmutableHashSet();
        }

        private static void RemoveRedundantRelationships(HashSet<BinaryRelationship> relationships)
        {
            RemoveRedundantMatchingLessEquals(relationships);
            RemoveRedundantLessEquals(relationships);
            RemoveRedundantLessEqualWithNotEqual(relationships);
        }

        private static void RemoveRedundantLessEquals(HashSet<BinaryRelationship> relationships)
        {
            // a <= b && a < b  ->  a < b
            var leComparisions = GetComparisions(relationships, ComparisonKind.LessOrEqual);
            var lComparisions = GetComparisions(relationships, ComparisonKind.Less);
            relationships.ExceptWith(leComparisions.Where(le => lComparisions.Any(l => le.AreOperandsMatching(l))));

            // a <= b && a == b  ->  a == b
            leComparisions = GetComparisions(relationships, ComparisonKind.LessOrEqual);
            var equals = relationships.OfType<EqualsRelationship>();
            relationships.ExceptWith(leComparisions.Where(le => equals.Any(e => le.AreOperandsMatching(e))));
        }

        private static void RemoveRedundantLessEqualWithNotEqual(HashSet<BinaryRelationship> relationships)
        {
            // a <= b && a != b  ->  a < b

            var leComparisions = GetComparisions(relationships, ComparisonKind.LessOrEqual);
            var notEquals = relationships.OfType<ValueNotEqualsRelationship>();

            var toChange = new List<ComparisonRelationship>();
            for (int i = 0; i < leComparisions.Count; i++)
            {
                var leComparision = leComparisions[i];
                var matchingPairs = notEquals.Where(o => o.AreOperandsMatching(leComparision)).ToList();
                if (matchingPairs.Any())
                {
                    relationships.ExceptWith(matchingPairs);
                    toChange.Add(leComparision);
                }
            }

            foreach (var item in toChange)
            {
                relationships.Remove(item);
                relationships.Add(new ComparisonRelationship(ComparisonKind.Less, item.LeftOperand, item.RightOperand));
            }
        }

        private static void RemoveRedundantMatchingLessEquals(HashSet<BinaryRelationship> relationships)
        {
            // a <= b && b <= a  ->  a == b

            var leComparisions = GetComparisions(relationships, ComparisonKind.LessOrEqual);

            var toChange = new List<ComparisonRelationship>();
            for (int i = 0; i < leComparisions.Count; i++)
            {
                var leComparision = leComparisions[i];
                var matchingPairs = leComparisions.Where(o => o.AreOperandsSwapped(leComparision)).ToList();
                if (matchingPairs.Any())
                {
                    relationships.ExceptWith(matchingPairs);
                    toChange.Add(leComparision);
                }
            }

            foreach (var item in toChange)
            {
                relationships.Remove(item);
                relationships.Add(new ValueEqualsRelationship(item.LeftOperand, item.RightOperand));
            }
        }

        private static List<ComparisonRelationship> GetComparisions(HashSet<BinaryRelationship> relationships, ComparisonKind kind)
        {
            return relationships
                .OfType<ComparisonRelationship>()
                .Where(c => c.ComparisonKind == kind)
                .ToList();
        }

        private bool IsRelationshipOnLocalValues(BinaryRelationship relationship)
        {
            var localValues = Values.Values
                .Except(ProtectedSymbolicValues)
                .Concat(DistinguishedReferences)
                .ToImmutableHashSet();

            return localValues.Contains(relationship.LeftOperand) &&
                localValues.Contains(relationship.RightOperand);
        }

        internal ProgramState(ImmutableDictionary<ISymbol, SymbolicValue> values,
            ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraints,
            ImmutableDictionary<ProgramPoint, int> programPointVisitCounts,
            ImmutableStack<SymbolicValue> expressionStack,
            ImmutableHashSet<BinaryRelationship> relationships)
        {
            Values = values;
            Constraints = constraints;
            ProgramPointVisitCounts = programPointVisitCounts;
            ExpressionStack = expressionStack;
            Relationships = relationships;
        }

        public ProgramState PushValue(SymbolicValue symbolicValue)
        {
            return new ProgramState(
                Values,
                Constraints,
                ProgramPointVisitCounts,
                ExpressionStack.Push(symbolicValue),
                Relationships);
        }

        public ProgramState PushValues(IEnumerable<SymbolicValue> values)
        {
            if (!values.Any())
            {
                return this;
            }

            return new ProgramState(
                Values,
                Constraints,
                ProgramPointVisitCounts,
                ImmutableStack.Create(ExpressionStack.Concat(values).ToArray()),
                Relationships);
        }

        public ProgramState PopValue()
        {
            SymbolicValue poppedValue;
            return PopValue(out poppedValue);
        }

        public ProgramState PopValue(out SymbolicValue poppedValue)
        {
            return new ProgramState(
                Values,
                Constraints,
                ProgramPointVisitCounts,
                ExpressionStack.Pop(out poppedValue),
                Relationships);
        }

        public ProgramState PopValues(int numberOfValuesToPop)
        {
            if (numberOfValuesToPop <= 0)
            {
                return this;
            }

            var newStack = ImmutableStack.CreateRange(
                ExpressionStack.Skip(numberOfValuesToPop).Reverse());

            return new ProgramState(
                Values,
                Constraints,
                ProgramPointVisitCounts,
                newStack,
                Relationships);
        }

        public SymbolicValue PeekValue()
        {
            return ExpressionStack.Peek();
        }

        public SymbolicValue PeekValue(int nth)
        {
            return ExpressionStack.ToList()[nth];
        }

        internal bool HasValue => !ExpressionStack.IsEmpty;

        internal ProgramState StoreSymbolicValue(ISymbol symbol, SymbolicValue newSymbolicValue)
        {
            return new ProgramState(
                Values.SetItem(symbol, newSymbolicValue),
                Constraints,
                ProgramPointVisitCounts,
                ExpressionStack,
                Relationships);
        }

        public SymbolicValue GetSymbolValue(ISymbol symbol)
        {
            if (symbol != null &&
                Values.ContainsKey(symbol))
            {
                return Values[symbol];
            }
            return null;
        }

        internal ProgramState AddVisit(ProgramPoint visitedProgramPoint)
        {
            var visitCount = GetVisitedCount(visitedProgramPoint);
            return new ProgramState(
                Values,
                Constraints,
                ProgramPointVisitCounts.SetItem(visitedProgramPoint, visitCount + 1),
                ExpressionStack,
                Relationships);
        }

        internal int GetVisitedCount(ProgramPoint programPoint)
        {
            int value;
            if (!ProgramPointVisitCounts.TryGetValue(programPoint, out value))
            {
                value = 0;
            }

            return value;
        }

        internal ProgramState RemoveSymbols(Func<ISymbol, bool> predicate)
        {
            var cleanedValues = Values
                .Where(kv => !predicate(kv.Key))
                .ToImmutableDictionary();

            // SVs for live symbols
            var usedSymbolicValues = cleanedValues.Values
                .Concat(DistinguishedReferences)
                .Concat(cleanedValues.Values.OfType<NullableSymbolicValue>().Select(x => x.WrappedValue)) // Do not lose constraints on wrapped SV
                .ToImmutableHashSet();

            var cleanedConstraints = Constraints
                .Where(kv =>
                    usedSymbolicValues.Contains(kv.Key) ||
                    ProtectedSymbolicValues.Contains(kv.Key) ||
                    ExpressionStack.Contains(kv.Key))
                .ToImmutableDictionary();

            // Relationships for live symbols (no transitivity, so both of them need to be live in order to hold any information)
            var cleanedRelationships = Relationships
                .Where(r =>
                    usedSymbolicValues.Contains(r.LeftOperand) ||
                    usedSymbolicValues.Contains(r.RightOperand))
                .ToImmutableHashSet();

            return new ProgramState(cleanedValues, cleanedConstraints, ProgramPointVisitCounts, ExpressionStack, cleanedRelationships);
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            ProgramState p = obj as ProgramState;
            return Equals(p);
        }

        public bool Equals(ProgramState other)
        {
            if (other == null)
            {
                return false;
            }

            return DictionaryHelper.DictionaryEquals(Values, other.Values) &&
                DictionaryHelper.DictionaryEquals(Constraints, other.Constraints) &&
                Enumerable.SequenceEqual(ExpressionStack, other.ExpressionStack) &&
                Relationships.SetEquals(other.Relationships);
        }

        public override int GetHashCode()
        {
            var hash = 19;

            foreach (var symbolValueAssociation in Values)
            {
                hash = hash * 31 + symbolValueAssociation.Key.GetHashCode();
                hash = hash * 31 + symbolValueAssociation.Value.GetHashCode();
            }

            foreach (var constraint in Constraints)
            {
                hash = hash * 31 + constraint.Key.GetHashCode();
                hash = hash * 31 + constraint.Value.GetHashCode();
            }

            foreach (var value in ExpressionStack)
            {
                hash = hash * 31 + value.GetHashCode();
            }

            foreach (var relationship in Relationships)
            {
                hash = hash * 31 + relationship.GetHashCode();
            }

            return hash;
        }
    }
}
