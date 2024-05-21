using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;

namespace NullableReferences
{
    public class ModelUsedInController
    {
        // if a string property is not set, and nullable context is disabled there's a validation error.
        // Otherwise, strings behave always as nullable reference types.
#nullable enable
        public string NonNullableReferenceProperty { get; set; }
        [Required] public string RequiredNonNullableReferenceProperty { get; set; }
        public string? NullableReferenceProperty { get; set; }
#nullable disable
        public string ReferenceProperty { get; set; }
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
        [Required] public TStruct RequiredStructProperty { get; set; }
        public TStruct? NullableStructProperty { get; set; }
        public TNotNull NotNullProperty { get; set; }                           // Noncompliant
    }

    public class ControllerClass : Controller
    {
        [HttpPost] public IActionResult Create(GenericType<object, string, int, DateTime> model) => View(model);
    }
}
