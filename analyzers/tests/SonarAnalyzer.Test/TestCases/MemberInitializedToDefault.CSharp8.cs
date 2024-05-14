using System;
using System.Collections.Generic;

#nullable enable

namespace Tests.Diagnostics
{
    public class C
    {
        // If the field is not initialized and nullable is set to enabled then
        // there's a warning "CS8618: Non-nullable property 'MyProperty' must contain a non-null value when exiting constructor. Consider declaring the property as nullable."
        // To silence the warning one needs to use a field initializer and the suppression operator. We should not raise and issue in the case ! is present.
        public object Noncompliant { get; set; } = default; // FN
        public object MyProperty { get; set; } = default!;  // Compliant
        public object MyPropertyNull { get; set; } = null!; // Compliant
    }
}
