using System;
using System.Windows.Markup;

namespace Tests.Diagnostics
{
    public class MyExtension : MarkupExtension
    {
        public MyExtension() { }

        public MyExtension(object value1, object value2)
        {
            Value1 = value1;
            Value2 = value2;
        }

        [ConstructorArgument("value1")]
        public object Value1 { get; set; }

        [System.Windows.Markup.ConstructorArgument("value2")]
        public object Value2 { get; set; }

        [ConstructorArgument("value3")]  // Noncompliant {{Change this 'ConstructorArgumentAttribute' value to match one of the existing constructors arguments.}}
//                           ^^^^^^^^
        public object Value3 { get; set; }

        [ConstructorArgument] // Invalid syntax - argument is mandatory - do not raise
        public object Value4 { get; set; }

        [ConstructorArgument("foo")]
        [ConstructorArgument("bar")] // Invalid syntax - only 1 attribute allowed - do not raise
        public object Value4 { get; set; }
    }
}
