using System.Diagnostics.Contracts;

namespace CSharpLatest.CSharp9
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
