using System.Diagnostics.Contracts;

namespace CSharpLatest.CSharp9Features;

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
