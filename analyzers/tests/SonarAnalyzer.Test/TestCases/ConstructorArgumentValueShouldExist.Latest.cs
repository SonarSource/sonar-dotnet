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
