using System.Collections.Immutable;
using System.Linq;
using SonarAnalyzer.SymbolicExecution.Relationships;

namespace SonarAnalyzer.SymbolicExecution.Constraints
{
    public static class CollectionConstraintExtensions
    {
        public static ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> AddConstraintForSymbolicValue(
            this ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintMap,
            SymbolicValue symbolicValue, SymbolicValueConstraint constraint)
        {
            var constraints = ImmutableDictionary.GetValueOrDefault(constraintMap, symbolicValue);

            var updatedConstraints = constraints != null
                ? constraints.WithConstraint(constraint)
                : SymbolicValueConstraints.Create(constraint);

            return constraintMap.SetItem(symbolicValue, updatedConstraints);
        }

        public static ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> RemoveConstraintForSymbolicValue(
            this ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintMap,
            SymbolicValue symbolicValue, SymbolicValueConstraint constraint)
        {
            var constraints = ImmutableDictionary.GetValueOrDefault(constraintMap, symbolicValue);

            if (constraints == null)
            {
                return constraintMap;
            }

            var updatedConstraints = constraints.WithoutConstraint(constraint);

            return constraintMap.SetItem(symbolicValue, updatedConstraints);
        }

        public static ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> AddConstraintTo<TRelationship>(
            this ImmutableDictionary<SymbolicValue, SymbolicValueConstraints> constraintsMap,
            SymbolicValue symbolicValue, SymbolicValueConstraint constraint, ProgramState programState)
            where TRelationship : BinaryRelationship
        {
            var newConstraintsMap = constraintsMap;
            var equalSymbols = programState.Relationships
                            .OfType<TRelationship>()
                            .Select(r => GetOtherOperandFromMatchingRelationship(symbolicValue, r))
                            .Where(e => e != null);

            foreach (var equalSymbol in equalSymbols.Where(e => !programState.HasConstraint(e, constraint)))
            {
                newConstraintsMap = newConstraintsMap.AddConstraintForSymbolicValue(equalSymbol, constraint);
            }

            return newConstraintsMap;
        }

        private static SymbolicValue GetOtherOperandFromMatchingRelationship(SymbolicValue symbolicValue,
            BinaryRelationship relationship)
        {
            if (relationship.RightOperand == symbolicValue)
            {
                return relationship.LeftOperand;
            }
            else if (relationship.LeftOperand == symbolicValue)
            {
                return relationship.RightOperand;
            }
            else
            {
                return null;
            }
        }
    }

}
