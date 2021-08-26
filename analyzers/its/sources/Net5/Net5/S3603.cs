using System.Diagnostics.Contracts;

namespace Net5
{
    record Record
    {
        Record()
        {
            [Pure]
            void LocalFunction()
            {
                [Pure]
                void NestedLocalFunction()
                {
                }
            }
        }
    }
}
