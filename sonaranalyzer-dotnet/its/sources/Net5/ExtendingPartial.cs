using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net5
{
    public partial class C
    {
        // Okay because no definition is required here
        partial void M1();

        // Okay because M2 has a definition
        private partial void M2();

    }

    public partial class C
    {
        private partial void M2() { }
    }

    public partial class D
    {
        // Okay
        internal partial bool TryParse(string s, out int i);
    }

    public partial class D
    {
        internal partial bool TryParse(string s, out int i) { i = 1; return true; }
    }

    public interface IStudent
    {
        string GetName();
    }

    public partial class PartialStudent : IStudent
    {
        public virtual partial string GetName();
    }

    public partial class PartialStudent
    {
        public virtual partial string GetName() => "X";
    }
}
