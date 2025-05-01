using System.Collections.Generic;
using System.Linq;

public class Sample
{
    public IEnumerable<int> Query(IEnumerable<string> list) =>
        from x in list      // Noncompliant {{Use IEnumerable extensions instead of the query syntax.}}
        where x != "test"
        select x.Length;

    public IEnumerable<int> Extensions(IEnumerable<string> list) =>
        list.Where(x => x != "test").Select(x => x.Length);
}
