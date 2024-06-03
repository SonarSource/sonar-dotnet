using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CSharp9
{
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding#constructor-binding-and-record-types
    public record RecordModel(
        int ValueProperty,                                                         // Noncompliant
        [property: JsonRequired] int RequiredValueProperty,                        // Without the property prefix the attribute would have been applied to the constructor parameter instead
        [Range(0, 2)] int RequiredValuePropertyWithoutPropertyPrefix,              // Noncompliant, FP see: https://github.com/SonarSource/sonar-dotnet/issues/9363
        int? NullableValueProperty,
        int PropertyWithDefaultValue = 42);

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
