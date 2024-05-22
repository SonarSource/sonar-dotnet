using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CSharp9
{
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding#constructor-binding-and-record-types
    public record RecordModel(
        int ValueProperty,                                                          // Noncompliant
        [property: JsonRequired] int RequiredValueProperty,                             // without the property prefix the attribute would have been applied to the constructor parameter instead
        int? NullableValueProperty);

    public class ModelUsedInController
    {
        public int PropertyWithInit { get; init; }                                  // Noncompliant
    }

    public class DerivedFromController : Controller
    {
        [HttpGet] public IActionResult Read(RecordModel model) => View(model);
        [HttpPost] public IActionResult Create(ModelUsedInController model) => View(model);
    }
}
