using System.Collections;
using System.Collections.Generic;

public class CollectionWithArguments : IEnumerable<int>
{
    public CollectionWithArguments(string a, params object[] b) { }

    public CollectionWithArguments(object a, object b, object c) { }

    public void Add(int item) { }

    public IEnumerator<int> GetEnumerator() => throw null;

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public void Method()
    {
        _ = new CollectionWithArguments("", null, null); // Noncompliant
        CollectionWithArguments collection = [with("", null, null), 1]; // FN NET-4937
    }
}
