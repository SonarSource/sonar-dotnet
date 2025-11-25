using System;
using System.Windows.Markup;

public class MyExtension3 : MarkupExtension
{
    public MyExtension3(object value1) { Value1 = value1; }

    [ConstructorArgument("value2")]  // Noncompliant {{Change this 'ConstructorArgumentAttribute' value to match one of the existing constructors arguments.}}
    public object Value1 { get; set; }

    public override object ProvideValue(IServiceProvider serviceProvider) => null;
}

// This is fake scaffolding to avoid .NET 5 reference issue.
// https://github.com/SonarSource/sonar-dotnet/issues/3425
namespace System.Windows.Markup
{
    public interface IServiceProvider { }

    public abstract class MarkupExtension
    {
        public abstract object ProvideValue(IServiceProvider serviceProvider);
    }

    public class ConstructorArgumentAttribute : Attribute
    {
        public ConstructorArgumentAttribute(string argumentName) { }
    }
}

namespace CSharp13
{
    //https://sonarsource.atlassian.net/browse/NET-553
    public partial class MyExtension3 : MarkupExtension
    {
        public MyExtension3(object value1) { Value1 = value1; }
        public override object ProvideValue(IServiceProvider serviceProvider) => null;
        [ConstructorArgument("value2")] // Noncompliant
        public partial object Value1 { get; set; }
    }
}

namespace CSharp14
{
    public partial class NonCompliantPartialConstructor : MarkupExtension
    {
        public partial NonCompliantPartialConstructor(object value1);
        [ConstructorArgument("value2")]     // Noncompliant
        public object Value1 { get; set; }
    }

    public partial class CompliantPartialConstructor : MarkupExtension
    {
        public partial CompliantPartialConstructor(object value1);
        [ConstructorArgument("value1")]
        public object Value1 { get; set; }
    }

    public static class NonCompliantExtensions
    {

        extension(NonCompliantStaticExtensionProperty ext)
        {
            [ConstructorArgument("value2")]             // Noncompliant
            public static object Value1 => 42;
        }

        extension(NonCompliantInstanceExtensionProperty ext)
        {
            [ConstructorArgument("value2")]             // Noncompliant
            public object Value1 => 42;
        }
    }

    public static class CompliantExtensions
    {
        extension(CompliantStaticExtensionProperty ext)
        {
            [ConstructorArgument("value1")]             // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2696
            public static object Value1 => 42;
        }

        extension(CompliantInstanceExtensionProperty ext)
        {
            [ConstructorArgument("value1")]             // Noncompliant FP https://sonarsource.atlassian.net/browse/NET-2696
            public object Value1 => 42;
        }
    }

    public class NonCompliantStaticExtensionProperty : MarkupExtension
    {
        public NonCompliantStaticExtensionProperty(object value1) { this.Value1 = value1; }   // Error [CS9286] {{'NonCompliantStaticExtensionProperty' does not contain a definition for 'Value1' and no accessible extension member 'Value1' for receiver of type 'NonCompliantStaticExtensionProperty' could be found (are you missing a using directive or an assembly reference?)}}
        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }

    public class NonCompliantInstanceExtensionProperty : MarkupExtension
    {
        public NonCompliantInstanceExtensionProperty(object value1) { this.Value1 = value1; } // Error [CS0200] {{Property or indexer 'NonCompliantExtensions.extension(NonCompliantInstanceExtensionProperty).Value1' cannot be assigned to -- it is read only}}
        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }


    public class CompliantStaticExtensionProperty : MarkupExtension
    {
        public CompliantStaticExtensionProperty(object value1) { this.Value1 = value1; }      // Error [CS9286] {{'CompliantStaticExtensionProperty' does not contain a definition for 'Value1' and no accessible extension member 'Value1' for receiver of type 'CompliantStaticExtensionProperty' could be found (are you missing a using directive or an assembly reference?)}}
        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }

    public class CompliantInstanceExtensionProperty : MarkupExtension
    {
        public CompliantInstanceExtensionProperty(object value1) { this.Value1 = value1; }    // Error [CS0200] {{Property or indexer 'CompliantExtensions.extension(CompliantInstanceExtensionProperty).Value1' cannot be assigned to -- it is read only}}
        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
}
