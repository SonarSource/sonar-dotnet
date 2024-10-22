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
