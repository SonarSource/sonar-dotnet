using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Net5
{
    public class SkipLocalsInit
    {
        [SkipLocalsInit]
        public void Method()
        {
            int i; // not initialized to 0
        }
    }
}
