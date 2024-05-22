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
