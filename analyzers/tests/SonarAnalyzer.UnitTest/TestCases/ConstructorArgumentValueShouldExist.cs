using System;
using System.Windows.Markup;

namespace Tests.Diagnostics
{
    public class MyExtension1 : MarkupExtension
    {
        public MyExtension1(object value1) { Value1 = value1; }

        [ConstructorArgument("value1")]
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
    public class MyExtension2 : MarkupExtension
    {
        public MyExtension2(object value1) { Value1 = value1; }

        [System.Windows.Markup.ConstructorArgument("value1")]
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
    public class MyExtension3 : MarkupExtension
    {
        public MyExtension3(object value1) { Value1 = value1; }

        [ConstructorArgument("value2")]  // Noncompliant {{Change this 'ConstructorArgumentAttribute' value to match one of the existing constructors arguments.}}
//                           ^^^^^^^^
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
    public class MyExtension4 : MarkupExtension
    {
        public MyExtension4(object value1) { Value1 = value1; }

        [ConstructorArgument] // Error [CS7036] - Invalid syntax - argument is mandatory - do not raise
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
    public class MyExtension5 : MarkupExtension
    {
        public MyExtension5(object value1) { Value1 = value1; }

        [ConstructorArgument("foo")]
        [ConstructorArgument("bar")] // Error [CS0579] - Invalid syntax - only 1 attribute allowed - do not raise
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
}
