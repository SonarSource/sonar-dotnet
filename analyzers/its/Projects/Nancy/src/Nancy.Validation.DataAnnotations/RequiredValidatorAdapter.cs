﻿namespace Nancy.Validation.DataAnnotations
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.ComponentModel.DataAnnotations;

    using Nancy.Validation.Rules;

    /// <summary>
    /// An adapter for the <see cref="System.ComponentModel.DataAnnotations.RequiredAttribute"/>.
    /// </summary>
    public class RequiredValidatorAdapter : DataAnnotationsValidatorAdapter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequiredValidatorAdapter"/> class.
        /// </summary>
        public RequiredValidatorAdapter() : base("Required")
        {
        }

        /// <summary>
        /// Gets a boolean that indicates if the adapter can handle the
        /// provided <paramref name="attribute"/>.
        /// </summary>
        /// <param name="attribute">The <see cref="ValidationAttribute"/> that should be handled.</param>
        /// <returns><see langword="true" /> if the attribute can be handles, otherwise <see langword="false" />.</returns>
        public override bool CanHandle(ValidationAttribute attribute)
        {
            return attribute is RequiredAttribute;
        }

        /// <summary>
        /// Gets the rules the adapter provides.
        /// </summary>
        /// <param name="attribute">The <see cref="ValidationAttribute"/> that should be handled.</param>
        /// <param name="descriptor">A <see cref="PropertyDescriptor"/> instance for the property that is being validated.</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of <see cref="ModelValidationRule"/> instances.</returns>
        public override IEnumerable<ModelValidationRule> GetRules(ValidationAttribute attribute, PropertyDescriptor descriptor)
        {
            var requiredAttribute = (RequiredAttribute) attribute;

            yield return new NotNullValidationRule(attribute.FormatErrorMessage, new[] { descriptor.Name });

            if (!requiredAttribute.AllowEmptyStrings)
            {
                yield return new NotEmptyValidationRule(attribute.FormatErrorMessage, new[] { descriptor.Name });
            }
        }
    }
}
