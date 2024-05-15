using System;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

namespace CSharp11
{
    // Repro https://github.com/SonarSource/sonar-dotnet/issues/9275
    public class Model
    {
        public int ValueProperty { get; set; }        // Noncompliant

        [Custom]                                      // Noncompliant
        public int ValuePropertyAnnotatedWithCustomAttribute { get; set; }

        [JsonRequired]                                // Noncompliant - FP because the attribute is annotated with JsonRequiredAttribute
        public int AnotherValueProperty { get; set; }
    }

    public class DerivedFromController : Controller
    {
        [HttpPost] public IActionResult Create(Model model) => View(model);
    }

    public class CustomAttribute : Attribute { }
}
