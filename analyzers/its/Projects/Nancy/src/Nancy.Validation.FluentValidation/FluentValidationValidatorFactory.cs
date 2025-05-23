﻿namespace Nancy.Validation.FluentValidation
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using global::FluentValidation;

    /// <summary>
    /// Creates and <see cref="IValidator"/> for Fluent Validation.
    /// </summary>
    public class FluentValidationValidatorFactory : IModelValidatorFactory
    {
        private readonly IFluentAdapterFactory adapterFactory;
        private readonly IEnumerable<IValidator> validators;

        /// <summary>
        /// Initializes a new instance of the <see cref="FluentValidationValidatorFactory"/> instance, with the
        /// provided <see cref="IFluentAdapterFactory"/>.
        /// </summary>
        /// <param name="adapterFactory">The factory that should be usdd to create <see cref="IFluentAdapter"/> instances.</param>
        /// <param name="validators">The <see cref="IValidator"/> instance that are available for validation.</param>
        public FluentValidationValidatorFactory(IFluentAdapterFactory adapterFactory, IEnumerable<IValidator> validators)
        {
            this.adapterFactory = adapterFactory;
            this.validators = validators;
        }

        /// <summary>
        /// Creates a <see cref="IModelValidator"/> instance for the given type.
        /// </summary>
        /// <param name="type">The type of the model that is being validated.</param>
        /// <returns>An <see cref="IModelValidator"/> instance. If no fluent validation rules were found for the specified <paramref name="type"/> then <see langword="null"/> is returned.</returns>
        public IModelValidator Create(Type type)
        {
            var instance =
                GetValidatorInstance(type);

            return (instance != null) ?
                new FluentValidationValidator(instance, this.adapterFactory, type) :
                null;
        }

        private IValidator GetValidatorInstance(Type type)
        {
            var fullType =
                CreateValidatorType(type);
            var available = this.validators
                .Where(validator => fullType.GetTypeInfo().IsAssignableFrom(validator.GetType()))
                .ToArray();

            if (available.Length > 1)
            {
                var names = string.Join(", ", available.Select(v => v.GetType().Name));
                var message = string.Concat(
                    "Ambiguous choice between multiple validators for type ",
					type.Name,
					". The validators available are: ",
					names);

                throw new InvalidOperationException(message);
            }

            return available.FirstOrDefault();
        }

        private static Type CreateValidatorType(Type type)
        {
            return typeof(AbstractValidator<>).MakeGenericType(type);
        }
    }
}