using Microsoft.AspNetCore.Mvc;
using TestInstrumentation.ResponsibilitySpecificServices;

namespace CSharp13
{
    public partial class WithFieldBackedPartialProperties : ControllerBase // Noncompliant
    {
        public partial IS1 S1 { get; }
        public partial IS2 S2 { get; init; }
    }

    public partial class WithPartialIndexer : ControllerBase // Noncompliant
    {
        public partial int this[int i] { get; set; } 

    }
}
