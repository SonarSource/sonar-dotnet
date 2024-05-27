using System;
using System.Threading;
using System.Threading.Tasks;
using FluentValidation;
using FluentValidation.Results;

public class FluentValidationTests_IValidatorT
{
    public class IntAbstractValidator : AbstractValidator<int> { }

    public class IntIValidatorT : IValidator<int>
    {
        public ValidationResult Validate(int instance) => null;
        public Task<ValidationResult> ValidateAsync(int instance, CancellationToken cancellation = new CancellationToken()) => null;

        public ValidationResult Validate(IValidationContext context) => null;
        public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = new CancellationToken()) => null;
        public IValidatorDescriptor CreateDescriptor() => null;
        public bool CanValidateInstancesOfType(Type type) => true;
    }

    public class ExplicitIntIValidatorT : IValidator<int>
    {
        public ValidationResult Validate(int instance) => null;
        ValidationResult IValidator<int>.Validate(int instance) => null;
        Task<ValidationResult> IValidator<int>.ValidateAsync(int instance, CancellationToken cancellation = new CancellationToken()) => null;

        ValidationResult IValidator.Validate(IValidationContext context) => null;
        Task<ValidationResult> IValidator.ValidateAsync(IValidationContext context, CancellationToken cancellation = new CancellationToken()) => null;
        IValidatorDescriptor IValidator.CreateDescriptor() => null;
        bool IValidator.CanValidateInstancesOfType(Type type) => true;
    }

    public async Task Validate()
    {
        var intAbstractValidator = new IntAbstractValidator();
        intAbstractValidator.Validate(0); // Compliant

        var intIValidatorT = new IntIValidatorT();
        intIValidatorT.Validate(0); // Compliant

        var explicitIntIValidatorT = new ExplicitIntIValidatorT();
        explicitIntIValidatorT.Validate(0); // Compliant
        ((IValidator<int>)explicitIntIValidatorT).Validate(0); // Compliant
    }
}


public class FluentValidationTests_IValidator
{
    public class Validator : IValidator
    {
        public ValidationResult Validate(IValidationContext context) => null;
        public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = new CancellationToken()) => null;
        public IValidatorDescriptor CreateDescriptor() => null;
        public bool CanValidateInstancesOfType(Type type) => true;
    }

    public class ExplicitIValidator : IValidator
    {
        public ValidationResult Validate(IValidationContext context) => null;
        ValidationResult IValidator.Validate(IValidationContext context) => null;
        Task<ValidationResult> IValidator.ValidateAsync(IValidationContext context, CancellationToken cancellation = new CancellationToken()) => null;
        IValidatorDescriptor IValidator.CreateDescriptor() => null;
        bool IValidator.CanValidateInstancesOfType(Type type) => true;
    }

    public async Task Validate()
    {
        var validator = new Validator();
        validator.Validate(new ValidationContext<int>(0)); // Compliant

        var explicitIntIValidator = new ExplicitIValidator();
        explicitIntIValidator.Validate(new ValidationContext<int>(0)); // Compliant
        ((IValidator)explicitIntIValidator).Validate(new ValidationContext<int>(0)); // Compliant
    }
}

public class FluentValidationTests_NotFluentValidation
{
    public ValidationResult Validate(IValidationContext context) => null;
    public Task<ValidationResult> ValidateAsync(IValidationContext context, CancellationToken cancellation = new CancellationToken()) => null;

    public ValidationResult Validate<T>(T instance) => null;
    public Task<ValidationResult> ValidateAsync<T>(T instance, CancellationToken cancellation = new CancellationToken()) => null;

    public async Task Validate()
    {
        Validate(new ValidationContext<int>(0)); // Noncompliant This method does not implement IValidation
        Validate<int>(0);                        // Noncompliant This method does not implement IValidation<T>
    }
}
