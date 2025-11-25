using System;
using System.Windows.Markup;

namespace CSharp13
{
    public partial class MyExtension3 : MarkupExtension
    {
        public partial object Value1 // Compliant
        {
            get => new object();
            set { }
        }
    }
}

namespace CSharp14
{
    public partial class NonCompliantPartialConstructor : MarkupExtension
    {
        public partial NonCompliantPartialConstructor(object value1) { Value1 = value1; }
        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }

    public partial class CompliantPartialConstructor : MarkupExtension
    {
        public partial CompliantPartialConstructor(object value1) { Value1 = value1; }
        public override object ProvideValue(IServiceProvider serviceProvider) => null;
    }
}
