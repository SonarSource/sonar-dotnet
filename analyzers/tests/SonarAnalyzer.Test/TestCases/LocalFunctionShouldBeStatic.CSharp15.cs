using System.Collections.Generic;
using System.Linq;

static class Extensions
{
    extension(IEnumerable<int> source)
    {
        public int this[int index]
        {
            get
            {
                int Count() => source.Count(); // Compliant
                return Count() + index;
            }
        }
    }
}

class CollectionExpressionArguments
{
    List<int> M(int capacity)
    {
        List<int> Create() => [with(capacity), 1]; // Compliant
        return Create();
    }
}
