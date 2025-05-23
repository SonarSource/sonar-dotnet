﻿namespace Nancy.Routing.Constraints
{
    /// <summary>
    /// Constraint for <see cref="int"/> route segments with a maximum value.
    /// </summary>
    public class MaxRouteSegmentConstraint : ParameterizedRouteSegmentConstraintBase<int>
    {
        /// <summary>
        /// Gets the name of the constraint.
        /// </summary>
        /// <value>The constraint's name.</value>
        public override string Name
        {
            get { return "max"; }
        }

        /// <summary>
        /// Tries to match the given segment and parameters against the constraint.
        /// </summary>
        /// <param name="segment">The segment to match.</param>
        /// <param name="parameters">The parameters to match.</param>
        /// <param name="matchedValue">The matched value.</param>
        /// <returns>
        /// <see langword="true"/> if the segment matches the constraint, <see langword="false"/> otherwise.
        /// </returns>
        protected override bool TryMatch(string segment, string[] parameters, out int matchedValue)
        {
            int minValue;
            int intValue;

            if (!this.TryParseInt(parameters[0], out minValue) ||
                !this.TryParseInt(segment, out intValue))
            {
                matchedValue = default(int);
                return false;
            }

            if (intValue > minValue)
            {
                matchedValue = default(int);
                return false;
            }

            matchedValue = intValue;
            return true;
        }
    }
}