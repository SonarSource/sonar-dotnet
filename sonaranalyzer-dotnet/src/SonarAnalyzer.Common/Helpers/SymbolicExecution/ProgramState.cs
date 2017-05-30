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

namespace SonarAnalyzer.Helpers.FlowAnalysis.Common
{
    public class ProgramState : IEquatable<ProgramState>
    {
        internal ImmutableDictionary<ISymbol, SymbolicValue> Values { get; }
        internal ImmutableDictionary<SymbolicValue, SymbolicValueConstraint> Constraints { get; }
        internal ImmutableDictionary<ProgramPoint, int> ProgramPointVisitCounts { get; }
        internal ImmutableStack<SymbolicValue> ExpressionStack { get; }
        internal ImmutableHashSet<BinaryRelationship> Relationships { get; }

        private static ImmutableDictionary<SymbolicValue, SymbolicValueConstraint> InitialConstraints =>
            new Dictionary<SymbolicValue, SymbolicValueConstraint>
            {
                { SymbolicValue.True, BoolConstraint.True },
                { SymbolicValue.False, BoolConstraint.False },
                { SymbolicValue.Null, ObjectConstraint.Null },
                { SymbolicValue.This, ObjectConstraint.NotNull },
                { SymbolicValue.Base, ObjectConstraint.NotNull }
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

        public bool FailedAssertState { get; }

        internal ProgramState()
            : this(ImmutableDictionary<ISymbol, SymbolicValue>.Empty,
                  InitialConstraints,
                  ImmutableDictionary<ProgramPoint, int>.Empty,
                  ImmutableStack<SymbolicValue>.Empty,
                  ImmutableHashSet<BinaryRelationship>.Empty,
                  false)
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
                relationships,
                FailedAssertState);
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
            ImmutableDictionary<SymbolicValue, SymbolicValueConstraint> constraints,
            ImmutableDictionary<ProgramPoint, int> programPointVisitCounts,
            ImmutableStack<SymbolicValue> expressionStack,
            ImmutableHashSet<BinaryRelationship> relationships,
            bool failedAssertState)
        {
            Values = values;
            Constraints = constraints;
            ProgramPointVisitCounts = programPointVisitCounts;
            ExpressionStack = expressionStack;
            Relationships = relationships;
            FailedAssertState = failedAssertState;
        }

        public ProgramState PushValue(SymbolicValue symbolicValue)
        {
            return new ProgramState(
                Values,
                Constraints,
                ProgramPointVisitCounts,
                ExpressionStack.Push(symbolicValue),
                Relationships,
                FailedAssertState);
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
                Relationships,
                FailedAssertState);
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
                Relationships,
                FailedAssertState);
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
                Relationships,
                FailedAssertState);
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
                Relationships,
                FailedAssertState);
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
                Relationships,
                FailedAssertState);
        }

        internal ProgramState SetFailedAssert()
        {
            return new ProgramState(
                Values,
                Constraints,
                ProgramPointVisitCounts,
                ExpressionStack,
                Relationships,
                failedAssertState: true);
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

            return new ProgramState(
                cleanedValues, 
                cleanedConstraints, 
                ProgramPointVisitCounts, 
                ExpressionStack, 
                cleanedRelationships,
                FailedAssertState);
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
