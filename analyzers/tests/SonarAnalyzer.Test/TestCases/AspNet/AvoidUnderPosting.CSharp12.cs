using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CSharp12
{
    public class ModelWithPrimaryConstructor(int vp, int rvp, int nvp)
    {
        public int ValueProperty { get; set; } = vp;                        // Compliant: no parameterless constructor, type cannot be used for Model Binding
    }

    public class ModelWithPrimaryAndParameterlessConstructor(int vp, int rvp, int nvp)
    {
        public ModelWithPrimaryAndParameterlessConstructor() : this(0, 0, 0) { }
        public int ValueProperty { get; set; } = vp;                        // Noncompliant
    }

    public class DerivedFromController : Controller
    {
        [HttpPost] public IActionResult Create(ModelWithPrimaryConstructor model) => View(model);
        [HttpDelete] public IActionResult Remove(ModelWithPrimaryAndParameterlessConstructor model) => View(model);
    }
}

namespace Repro9275
{
    // Repro https://github.com/SonarSource/sonar-dotnet/issues/9275
    public class Model
    {
        public int ValueProperty { get; set; }              // Noncompliant

        [Custom]
        public int ValuePropertyAnnotatedWithCustomAttribute { get; set; } // Noncompliant

        [JsonRequired]                                      // Compliant - the property is annotated with JsonRequiredAttribute
        public int AnotherValueProperty { get; set; }

        public required int RequiredProperty { get; set; }  // Compliant - because the property has the required modifier
    }

    public class DerivedFromController : Controller
    {
        [HttpPost] public IActionResult Create(Model model) => View(model);
    }

    public class CustomAttribute : Attribute { }
}
