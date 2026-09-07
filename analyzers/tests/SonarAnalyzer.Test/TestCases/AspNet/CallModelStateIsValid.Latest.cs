using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Repro_NET1044
{
    public class PositionalPatternClause : Controller
    {
        public int Pattern((int X, int Y) tuple) => // Compliant
            tuple switch
            {
                (_, _) => 42     // This was throwing NullReferenceException
            };
    }
}

public sealed class AssetInput
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; }
}

public sealed class NestedAssetInput
{
    [Required]
    public string Name { get; set; }

    public AssetInput Asset { get; set; }
}

public sealed class AssetController : ControllerBase
{
    [HttpPost]
    public IActionResult Create(AssetInput input)                               // Compliant
    {
        var error = FirstValidationError(input);
        if (error is not null)
            return BadRequest(error);

        return Ok();
    }

    [HttpPost]
    public IActionResult CreateWithNamedArguments(AssetInput input)             // Compliant
    {
        var error = ValidationHelpers.FirstValidationError(input);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult DoesNotExit(AssetInput input)                          // Compliant
    {
        var error = FirstValidationError(input);
        if (error is not null)
        {
            BadRequest(error);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult DirectValidation(AssetInput input)                     // Compliant
    {
        var results = new List<ValidationResult>();
        if (!Validator.TryValidateObject(input, new ValidationContext(input), results, validateAllProperties: true))
        {
            return BadRequest(results);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult DirectValidationWithPositionalArgument(AssetInput input) // Compliant
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, true);
        return Ok();
    }

    [HttpPost]
    public IActionResult DirectValidationWithFalsePositionalArgument(AssetInput input) // Noncompliant
    {
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(input, new ValidationContext(input), results, false);
        return Ok();
    }

    [HttpPost]
    public IActionResult ValidatesAnotherModel(AssetInput input)                // Compliant
    {
        var error = FirstValidationError(new AssetInput());
        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult RequiresRecursiveValidation(NestedAssetInput input)    // Compliant
    {
        var error = FirstValidationError(input);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult ValidatesOnlyOneParameter(AssetInput first, AssetInput second) // Compliant
    {
        var error = FirstValidationError(first);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult DoesNotValidateAllProperties(AssetInput input)          // Noncompliant
    {
        var error = FirstRequiredValidationError(input);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult ValidatesReplacement(AssetInput input)                  // Compliant
    {
        var error = ReplacementValidationError(input);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult ParameterAttribute([Required] AssetInput input)         // Compliant
    {
        var error = FirstValidationError(input);
        if (error is not null)
        {
            return BadRequest(error);
        }

        return Ok();
    }

    [HttpPost]
    public IActionResult HelperChecksModelState(AssetInput input)                // Compliant
    {
        CheckModelState();
        return Ok();
    }

    [HttpPost]
    public IActionResult HelperCallsTryValidateModel(AssetInput input)           // Compliant
    {
        ValidateModel(input);
        return Ok();
    }

    private void CheckModelState() => _ = ModelState.IsValid;

    private void ValidateModel(object model) => TryValidateModel(model);

    private static string FirstValidationError(object model)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(model);
        return Validator.TryValidateObject(model, context, results, validateAllProperties: true)
            ? null
            : results[0].ErrorMessage;
    }

    private static string FirstRequiredValidationError(object model)
    {
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: false)
            ? null
            : results[0].ErrorMessage;
    }

    private static string ReplacementValidationError(object model)
    {
        model = new object();
        var results = new List<ValidationResult>();
        return Validator.TryValidateObject(model, new ValidationContext(model), results, validateAllProperties: true)
            ? null
            : results[0].ErrorMessage;
    }
}
