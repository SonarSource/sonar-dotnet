using System.Collections.Generic;
using System.Linq;

public class AsyncEnumerableOrdering
{
    public void Method(IEnumerable<int> source, IAsyncEnumerable<int> asyncSource)
    {
        source.OrderBy(x => x).Where(x => x > 0);           // Noncompliant {{"Where" should be used before "OrderBy"}}
                                                            // Secondary@-1 {{This "OrderBy" precedes a "Where" - you should change the order.}}
        asyncSource.OrderBy(x => x).Where(x => x > 0);      // FN NET-4995
        asyncSource.Where(x => x > 0).OrderBy(x => x);      // Compliant
    }
}
