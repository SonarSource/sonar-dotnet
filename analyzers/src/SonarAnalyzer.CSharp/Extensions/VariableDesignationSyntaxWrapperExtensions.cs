using System.Collections.Immutable;
using StyleCop.Analyzers.Lightup;

namespace SonarAnalyzer.Extensions
{
    public static class VariableDesignationSyntaxWrapperExtensions
    {
        /// <summary>
        /// Returns all <see cref="SingleVariableDesignationSyntaxWrapper"/> of the designation. Nested designations are flattened. For a designation like <c>(a, (b, c))</c>
        /// the method returns <c>[a, b, c]</c>. Only identifiers are included in the result and discards are ignored.
        /// </summary>
        /// <param name="variableDesignation">The designation to return the variables for.</param>
        /// <returns>A list of <see cref="SingleVariableDesignationSyntaxWrapper"/> that contain all flattened variables of the designation.</returns>
        public static ImmutableArray<SingleVariableDesignationSyntaxWrapper> AllVariables(this VariableDesignationSyntaxWrapper variableDesignation)
        {
            var builder = ImmutableArray.CreateBuilder<SingleVariableDesignationSyntaxWrapper>();
            CollectVariables(builder, variableDesignation);
            return builder.ToImmutableArray();

            static void CollectVariables(ImmutableArray<SingleVariableDesignationSyntaxWrapper>.Builder builder, VariableDesignationSyntaxWrapper variableDesignation)
            {
                if (ParenthesizedVariableDesignationSyntaxWrapper.IsInstance(variableDesignation))
                {
                    var parenthesized = (ParenthesizedVariableDesignationSyntaxWrapper)variableDesignation;
                    foreach (var variable in parenthesized.Variables)
                    {
                        CollectVariables(builder, variable);
                    }
                }
                else if (SingleVariableDesignationSyntaxWrapper.IsInstance(variableDesignation))
                {
                    builder.Add((SingleVariableDesignationSyntaxWrapper)variableDesignation);
                }
                // DiscardDesignationSyntaxWrapper.IsInstance(variableDesignation) are excluded
            }
        }
    }
}
