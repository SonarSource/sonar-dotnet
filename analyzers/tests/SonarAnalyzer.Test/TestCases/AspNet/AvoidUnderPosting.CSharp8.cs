using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

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
