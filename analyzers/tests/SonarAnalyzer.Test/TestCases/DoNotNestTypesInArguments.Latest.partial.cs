using System.Collections.Generic;

namespace Partial
{
    public partial class A
    {
        private partial void M(IDictionary<IList<int>, IList<string>> args); // Noncompliant;
    }
}
