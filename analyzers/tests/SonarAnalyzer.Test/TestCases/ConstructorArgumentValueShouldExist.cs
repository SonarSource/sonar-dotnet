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

        [ConstructorArgument("foo")] // Noncompliant
        [ConstructorArgument("bar")] // Noncompliant
                                     // Error@-1 [CS0579] - Invalid syntax - only 1 attribute allowed
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }

    public class MyExtension6 : MarkupExtension
    {
        public MyExtension6(object value1) { Value1 = value1; }

        [ConstructorArgument("v1", "v2")] // Error [CS1729] - 'ConstructorArgumentAttribute' does not contain a constructor that takes 2 arguments
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }

    public class MyExtension7 : MarkupExtension
    {
        public MyExtension7(object value1) { Value1 = value1; }

        [ConstructorArgument] // Error [CS7036]
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }

    public class MyExtension8 : MarkupExtension
    {
        public MyExtension8(object value1) { Value1 = value1; }

        [ConstructorArgument()] // Error [CS7036]
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }

    public class MyExtension9 : MarkupExtension
    {
        public MyExtension9(object value1) { Value1 = value1; }

        [ConstructorArgument(Property = "Test")] // Error [CS7036]
                                                 // Error@-1 [CS0246]
        public object Value1 { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
}
