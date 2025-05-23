﻿namespace Nancy.Routing.Constraints
{
    /// <summary>
    /// Constraint for route segments with a specific length.
    /// </summary>
    public class LengthRouteSegmentConstraint : ParameterizedRouteSegmentConstraintBase<string>
    {
        /// <summary>
        /// Gets the name of the constraint.
        /// </summary>
        /// <value>The constraint's name.</value>
        public override string Name
        {
            get { return "length"; }
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
        protected override bool TryMatch(string segment, string[] parameters, out string matchedValue)
        {
            int minLength;
            int maxLength;

            if (parameters.Length == 2)
            {
                if (!this.TryParseInt(parameters[0], out minLength) ||
                    !this.TryParseInt(parameters[1], out maxLength))
                {
                    matchedValue = null;
                    return false;
                }
            }
            else if (parameters.Length == 1)
            {
                minLength = 0;

                if (!this.TryParseInt(parameters[0], out maxLength))
                {
                    matchedValue = null;
                    return false;
                }
            }
            else
            {
                matchedValue = null;
                return false;
            }

            if (segment.Length < minLength || segment.Length > maxLength)
            {
                matchedValue = null;
                return false;
            }

            matchedValue = segment;
            return true;
        }
    }
}