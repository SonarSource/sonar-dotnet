using System;

namespace Net5
{
    public class AttributesOnLocalFunctions
    {
        public void Method()
        {
            LocalFunction(); // CS0612 'LocalFunction' is obsolete.

            [Obsolete]
            void LocalFunction() { }
        }
    }
}
