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
        public int ValueProperty { get; set; } = vp;                        // Compliant - the property has default value
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

namespace CSharp9
{
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding#constructor-binding-and-record-types
    public record RecordModel(
        int ValueProperty,                                                         // Noncompliant
        [property: JsonRequired] int RequiredValueProperty,                        // Without the property prefix the attribute would have been applied to the constructor parameter instead
        [Range(0, 2)] int RequiredValuePropertyWithoutPropertyPrefix,              // Noncompliant, FP the attribute is applied on the parameter and still works for the property see: https://github.com/SonarSource/sonar-dotnet/issues/9363
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

namespace CSharp8
{
    namespace NullableReferences
    {
        public class ModelUsedInController
        {
            #nullable enable
            public string NonNullableReferenceProperty { get; set; }  // Compliant - the JSON serializer will throw an exception if the value is missing from the request
            public object AnotherNonNullableReferenceProperty { get; set; }
            [Required] public string RequiredNonNullableReferenceProperty { get; set; }
            public string? NullableReferenceProperty { get; set; }
            public int ValueProperty { get; set; }                // Noncompliant
            public int? NullableValueProperty { get; set; }
            #nullable disable
            public string ReferenceProperty { get; set; }
            public object AnotherReferenceProperty { get; set; }
            public int? AnotherNullableValueProperty { get; set; }
        }

        public class DerivedFromController : Controller
        {
            [HttpPost]
            public IActionResult Create(ModelUsedInController model)
            {
                return View(model);
            }
        }
    }

    namespace CustomGenerics
    {
        public class GenericType<TNoContstraint, TClass, TStruct, TNotNull>
            where TClass : class
            where TStruct : struct
            where TNotNull : notnull
        {
            public TNoContstraint NoConstraintProperty { get; set; }
            public TClass ClassProperty { get; set; }
            public TStruct StructProperty { get; set; }                             // Noncompliant
            [JsonRequired] public TStruct RequiredStructProperty { get; set; }
            public TStruct? NullableStructProperty { get; set; }
            public TNotNull NotNullProperty { get; set; }                           // Noncompliant
        }

        public class ControllerClass : Controller
        {
            [HttpPost] public IActionResult Create(GenericType<object, string, int, DateTime> model) => View(model);
        }
    }
}

namespace CSharp13
{
    public partial class PartialPropertyClass
    {
        private int _testProperty { get; set; }
        public int TestProperty //Noncompliant
        {
            get { return _testProperty; }
            set { _testProperty = value; }
        }

        private int _partialProperty;
        public partial int PartialProperty // Compliant - raises on the definition
        {
            get => _partialProperty;
            set { _partialProperty = value; }
        }

        [JsonRequired]
        public partial int HasAttributePartialProperty // Compliant
        {
            get => _partialProperty;
            set { }
        }
        public partial int HasAttributePartialPropertyOther // Compliant
        {
            get => _partialProperty;
            set { }
        }
    }

    public class DerivedFromController : Controller
    {
        [HttpPost]
        public IActionResult Create(PartialPropertyClass model)
        {
            return View(model);
        }
    }
}
