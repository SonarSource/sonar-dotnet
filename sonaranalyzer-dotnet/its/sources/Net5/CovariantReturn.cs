using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Net5
{
    public class CovariantReturn
    {
        public virtual IEnumerable<int> OverrideMe(string m) => null;

        public IEnumerable<int> DoNotOverrideMe(string m) => null;
    }

    public class InheritCovariantReturn : CovariantReturn
    {
        // The feature is in progress
        // public override List<int> OverrideMe(string m) => null;

        // hides the member
        public new List<int> DoNotOverrideMe(string m) => null;
    }
}
