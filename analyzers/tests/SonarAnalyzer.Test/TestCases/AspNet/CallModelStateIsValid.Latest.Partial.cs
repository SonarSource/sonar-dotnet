using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public static class ValidationHelpers
{
    public static string FirstValidationError(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        return Validator.TryValidateObject(
                validationResults: results,
                instance: model,
                validateAllProperties: true,
                validationContext: context)
            ? null
            : results[0].ErrorMessage;
    }
}
