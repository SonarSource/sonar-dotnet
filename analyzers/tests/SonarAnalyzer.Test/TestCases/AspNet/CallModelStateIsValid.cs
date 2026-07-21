using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

public class NonCompliantController : ControllerBase
{
    [HttpGet("/[controller]")]
    public string Get([Required, FromQuery] string id)                          // Noncompliant {{ModelState.IsValid should be checked in controller actions.}}
    //            ^^^
    {
        return "Hello!";
    }

    [HttpPost("/[controller]")]
    public string Post(Movie movie)                                             // Noncompliant
    {
        return "Hello!";
    }

    [HttpPut("/[controller]")]
    public string Put(Movie movie)                                              // Noncompliant
    {
        return "Hello!";
    }

    [HttpDelete("/[controller]")]
    public string Delete(Movie movie)                                           // Noncompliant
    {
        return "Hello!";
    }

    [HttpPatch("/[controller]")]
    public string Patch(Movie movie)                                            // Noncompliant
    {
        return "Hello!";
    }

    [HttpGet]
    [HttpPost]
    [HttpPut]
    [HttpDelete]
    [HttpPatch]
    [Route("/[controller]/mix")]
    public string Mix([Required, FromQuery, EmailAddress] string email)         // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("GET")]
    [Route("/[controller]/accept-verbs")]
    public string AGet([Required] string id)                                    // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("POST")]
    [Route("/[controller]/accept-verbs")]
    public string APost(Movie movie)                                            // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("PUT")]
    [Route("/[controller]/accept-verbs")]
    public string APut(Movie movie)                                             // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("DELETE")]
    [Route("/[controller]/accept-verbs")]
    public string ADelete(Movie movie)                                          // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("PATCH")]
    [Route("/[controller]/accept-verbs")]
    public string APatch(Movie movie)                                           // Noncompliant
    {
        return "Hello!";
    }

    [AcceptVerbs("GET", "POST", "PUT", "DELETE", "PATCH")]
    [Route("/[controller]/many")]
    public string Many([Required, FromQuery, EmailAddress] string email)        // Noncompliant
    {
        return "Hello!";
    }

    [HttpGet("/[controller]/list")]
    public string[] List() => null;                                             // Compliant

    [HttpGet("/[controller]/listasync")]
    public Task<string[]> ListAsync(CancellationToken token) => null;           // Compliant
}

[ApiController]
public class ControllerWithApiAttributeAtTheClassLevel : ControllerBase
{
    [HttpPost("/[controller]")]
    public string Add(Movie movie)                                              // Compliant, ApiController attribute is applied at the class level.
    {
        return "Hello!";
    }
}

public class BaseClassHasApiControllerAttribute : ControllerWithApiAttributeAtTheClassLevel
{
    [HttpDelete("/[controller]")]
    public string Remove(Movie movie)                                           // Compliant, base class is decorated with the ApiController attribute
    {
        return "Hello!";
    }
}

[Controller]
public class ControllerThatDoesNotInheritFromControllerBase
{
    [HttpPost("/[controller]")]
    public string Add(Movie movie)                                              // Compliant, ModelState is not available in this context.
    {
        return "Hello!";
    }
}

public class SimpleController
{
    [HttpPost("/[controller]")]
    public string Add(Movie movie)                                              // Compliant, ModelState is not available in this context.
    {
        return "Hello!";
    }
}

public class CompliantController : ControllerBase
{
    [HttpGet("/[controller]")]
    public string Get([Required, FromQuery] string id)                          // Compliant
    {
        if (!ModelState.IsValid)
        {
            return null;
        }
        return "Hello";
    }

    [HttpGet("/[controller]/GetNoParam")]
    public string GetNoParam()                                                  // Compliant
    {
        return "Hello";
    }

    [HttpPost("/[controller]")]
    public string Post([Required, FromBody] string id)                          // Compliant
    {
        var state = ModelState;
        if (!state.IsValid)
        {
            return null;
        }
        return "Hello";
    }

    [HttpPost("[controller]/genre")]
    public string PostGenre([FromBody]Genre genre)                              // Compliant
    {
        return "Hello";
    }
}

[ValidateModel]
public class ControllerClassWithActionFilter : ControllerBase
{
    [HttpPost("/[controller]/create")]
    public string Create(Movie movie)                                           // Compliant, Controller class is decorated with an Action Filter
    {
        return "Hello!";
    }
}

public class ControllerMethodsWithActionFilter : ControllerBase
{
    [ValidateModel]
    [HttpPost("/[controller]/create")]
    public string Create(Movie movie)                                           // Compliant, Controller action is decorated with an Action Filter
    {
        return "Hello!";
    }
}

public class Genre
{
    public string Name { get; set; }
}

public class Movie
{
    [Required]
    public string Title { get; set; }

    [Range(1900, 2200)]
    public int Year { get; set; }
}

public class ValidateModelAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        if (!context.ModelState.IsValid)
        {
            context.Result = new BadRequestObjectResult(context.ModelState);
        }
    }
}

public class ControllerCallsTryValidate : ControllerBase
{
    [HttpPost]
    public string CallsTryValidateModel1([Required] string email)               // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email) ? "Hi!" : "Hello!";
    }

    [HttpPost]
    public string CallsTryValidateModel([Required] string email)                // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email, "prefix") ? "Hi!" : "Hello!";
    }
}

public class ControllerCallsTryValidateOverride : ControllerBase
{
    public override bool TryValidateModel(object model) => true;
    public override bool TryValidateModel(object model, string prefix) => true;

    [HttpPost]
    public string CallsTryValidateModel1([Required] string email)               // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email) ? "Hi!" : "Hello!";
    }

    [HttpPost]
    public string CallsTryValidateModel([Required] string email)                // Compliant, calls TryValidateModel
    {
        return TryValidateModel(email, "prefix") ? "Hi!" : "Hello!";
    }
}

public class InheritedValidationController : ControllerBase
{
    [HttpPost("/[controller]/inherited-member")]
    public string InheritedMemberAttribute(DerivedWithValidatedBase model)      // Noncompliant - a member inherited from the base class carries a validation attribute
    {
        return "Hello!";
    }

    [HttpPost("/[controller]/validatable")]
    public string ImplementsValidatable(ValidatableModel model)                 // Noncompliant - the model implements IValidatableObject
    {
        return "Hello!";
    }

    [HttpPost("/[controller]/validatable-base")]
    public string ImplementsValidatableViaBase(DerivedFromValidatableBase model) // Noncompliant - the model implements IValidatableObject through its base class
    {
        return "Hello!";
    }
}

public class InheritedValidationCompliantController : ControllerBase
{
    [HttpPost("/[controller]/inherited-member")]
    public string InheritedMemberAttribute(DerivedWithValidatedBase model)      // Compliant - ModelState.IsValid is checked
    {
        if (!ModelState.IsValid)
        {
            return null;
        }
        return "Hello!";
    }

    [HttpPost("/[controller]/validatable")]
    public string ImplementsValidatable(ValidatableModel model)                 // Compliant - ModelState.IsValid is checked
    {
        if (!ModelState.IsValid)
        {
            return null;
        }
        return "Hello!";
    }

    [HttpPost("/[controller]/validatable-base")]
    public string ImplementsValidatableViaBase(DerivedFromValidatableBase model) // Compliant - ModelState.IsValid is checked
    {
        if (!ModelState.IsValid)
        {
            return null;
        }
        return "Hello!";
    }
}

public class ValidatedBase
{
    [Required]
    public string Id { get; set; }
}

public class DerivedWithValidatedBase : ValidatedBase
{
    public string Name { get; set; }
}

public class ValidatableModel : IValidatableObject
{
    public string Name { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => new List<ValidationResult>();
}

public class ValidatableBase : IValidatableObject
{
    public string Name { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext) => new List<ValidationResult>();
}

public class DerivedFromValidatableBase : ValidatableBase
{
    public string Extra { get; set; }
}

public class TypeLevelValidationController : ControllerBase
{
    [HttpPost("/[controller]/custom-validation")]
    public string ClassLevelAttribute(TypeValidatedModel model)                 // Noncompliant - the model type itself is decorated with a validation attribute
    {
        return "Hello!";
    }

    [HttpPost("/[controller]/custom-validation-checked")]
    public string ClassLevelAttributeChecked(TypeValidatedModel model)          // Compliant - ModelState.IsValid is checked
    {
        if (!ModelState.IsValid)
        {
            return null;
        }
        return "Hello!";
    }
}

[CustomValidation(typeof(TypeValidatedModelValidator), nameof(TypeValidatedModelValidator.Validate))]
public class TypeValidatedModel
{
    public string Name { get; set; }
}

public static class TypeValidatedModelValidator
{
    public static ValidationResult Validate(TypeValidatedModel model, ValidationContext context) => ValidationResult.Success;
}

// https://github.com/SonarSource/sonar-dotnet/issues/9325
namespace Repro_9325
{
    public class MyController : ControllerBase
    {
        [HttpGet("/[controller]")]
        public string Get([Required, FromQuery] string id)                      // Noncompliant - FP: the Model State is checked in a method that's called from the Controller Action
        {
            if (!CheckModelState())
            {
                return "Error!";
            }
            return "Hello!";
        }

        private bool CheckModelState() => ModelState.IsValid;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9262
namespace Repro_9262
{
    public class MyController : ControllerBase
    {
        [HttpGet("/[controller]")]
        public string OnlyIngoredTypes(string str, object obj, dynamic dyn)
        {
            return "Hello!";
        }

        [HttpGet("/[controller]")]
        public string WithValidationAttribute(string str, [Required] object obj, dynamic dyn)   // Noncompliant
        {
            return "Hello!";
        }
    }
}

// NET-1586 / https://github.com/SonarSource/sonar-dotnet/issues/9262
namespace Repro_NET1586
{
    public class PrimitiveParametersController : ControllerBase
    {
        [HttpGet("/[controller]/session-error")]
        public IActionResult SessionErrorView(bool signedIn) => Ok();                           // Compliant - primitive parameter has no validation surface

        [HttpGet("/[controller]/numbers")]
        public IActionResult Numbers(int i, long l, double d, decimal m, char c) => Ok();       // Compliant - primitive parameters have no validation surface

        [HttpGet("/[controller]/structs")]
        public IActionResult Structs(Guid guid, DateTime dt, DateTimeOffset dto, TimeSpan ts) => Ok(); // Compliant - struct parameters have no validation surface

        [HttpGet("/[controller]/enum")]
        public IActionResult Enum(DayOfWeek day) => Ok();                                       // Compliant - enum parameter has no validation surface

        [HttpGet("/[controller]/nullable")]
        public IActionResult Nullable(int? i, bool? flag, Guid? guid) => Ok();                  // Compliant - nullable primitive/struct parameters have no validation surface
    }

    public class PrimitiveParametersWithAttributeController : ControllerBase
    {
        [HttpGet("/[controller]/bool")]
        public IActionResult Bool([Required] bool signedIn) => Ok();                            // Noncompliant - the parameter is decorated with a validation attribute

        [HttpGet("/[controller]/int")]
        public IActionResult Int([Range(1, 10)] int id) => Ok();                                // Noncompliant

        [HttpGet("/[controller]/guid")]
        public IActionResult Guid([Required] Guid guid) => Ok();                                // Noncompliant

        [HttpGet("/[controller]/datetime")]
        public IActionResult Date([Required] DateTime dt) => Ok();                              // Noncompliant

        [HttpGet("/[controller]/decimal")]
        public IActionResult Decimal([Range(0.0, 1.0)] decimal m) => Ok();                      // Noncompliant
    }

    public class CollectionParametersController : ControllerBase
    {
        [HttpPost("/[controller]/list-of-validated")]
        public string ListOfValidated(List<Movie> movies)              // Noncompliant - ASP.NET validates collection elements; Movie has [Required]
        {
            return "Hello!";
        }

        [HttpPost("/[controller]/array-of-validated")]
        public string ArrayOfValidated(Movie[] movies)                 // Noncompliant - ASP.NET validates array elements
        {
            return "Hello!";
        }

        [HttpPost("/[controller]/enumerable-of-validated")]
        public string EnumerableOfValidated(IEnumerable<Movie> movies) // Noncompliant - ASP.NET validates enumerable elements
        {
            return "Hello!";
        }

        [HttpPost("/[controller]/list-of-plain")]
        public string ListOfPlain(List<Genre> genres) => "Hello!";     // Compliant - element type has no validation surface
    }

    public class NestedModelController : ControllerBase
    {
        [HttpPost("/[controller]/nested-validated")]
        public string NestedValidated(OuterModel model) => "Hello!";   // Noncompliant - ASP.NET recurses into complex members; OuterModel.Inner is a Movie with [Required]

        [HttpPost("/[controller]/nested-plain")]
        public string NestedPlain(OuterPlainModel model) => "Hello!";  // Compliant - no member in the object graph carries a validation attribute

        [HttpPost("/[controller]/collection-of-nested-validated")]
        public string CollectionOfNestedValidated(IEnumerable<OuterModel> model) => "Hello!"; // Noncompliant - ASP.NET validates each element and recurses into OuterModel.Inner (Movie with [Required])

        [HttpPost("/[controller]/list-of-nested-validated")]
        public string ListOfNestedValidated(List<OuterModel> model) => "Hello!"; // Noncompliant - ASP.NET validates each element and recurses into OuterModel.Inner (Movie with [Required])

        [HttpPost("/[controller]/array-of-nested-validated")]
        public string ArrayOfNestedValidated(OuterModel[] model) => "Hello!"; // Noncompliant - ASP.NET validates each element and recurses into OuterModel.Inner (Movie with [Required])
    }

    public class OuterModel
    {
        public Movie Inner { get; set; }        // Movie has [Required]
    }

    public class OuterPlainModel
    {
        public Genre Inner { get; set; }        // Genre has no validation surface
    }

    public class RecursiveModelController : ControllerBase
    {
        [HttpPost("/[controller]/cyclic-plain")]
        public string CyclicPlain(SelfReferencingModel model) => "Hello!"; // Compliant - cyclic graph with no validation attribute (recursion must terminate via the visited guard)
    }

    public class SelfReferencingModel
    {
        public SelfReferencingModel Next { get; set; }  // self-reference
        public string Name { get; set; }
    }

    public class CustomCollectionController : ControllerBase
    {
        [HttpPost("/[controller]/custom-enumerable")]
        public string CustomEnumerable(MovieCollection movies) => "Hello!";                 // Noncompliant - custom IEnumerable<Movie> implementation; ASP.NET validates its elements

        [HttpPost("/[controller]/dictionary-of-validated")]
        public string DictionaryOfValidated(Dictionary<string, Movie> movies) => "Hello!";  // Noncompliant - ASP.NET validates dictionary values (Movie has [Required])
    }

    // Non-generic type that implements IEnumerable<Movie> directly: its element type is only reachable
    // through the implemented interface, not through its own type arguments or a Movie-typed member.
    public class MovieCollection : IEnumerable<Movie>
    {
        public IEnumerator<Movie> GetEnumerator() => null;
        IEnumerator IEnumerable.GetEnumerator() => null;
    }
}
