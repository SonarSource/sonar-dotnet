using System.Runtime.CompilerServices;

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
