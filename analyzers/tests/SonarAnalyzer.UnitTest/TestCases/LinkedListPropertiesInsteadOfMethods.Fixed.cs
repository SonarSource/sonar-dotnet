using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

class MyClass
{
    void Basic(LinkedList<int> data)
    {
        _ = data.First.Value; // Fixed
        _ = data.Last.Value; // Fixed
        data.First(x => x > 0);            // Compliant
        data.Last(x => x > 0);             // Compliant
        _ = data.First.Value;              // Compliant
        _ = data.Last.Value;               // Compliant
        data.Count();                      // Compliant
        data.Append(1).First().ToString(); // Compliant
        data.Append(1).Last().ToString();  // Compliant
        data?.First.Value.ToString();          // Fixed
        data?.Last.Value.ToString();           // Fixed

    }

    void CustomClass(LinkedList<int> data)
    {
        var classContainingLinkedList = new ClassContainingLinkedList();
        _ = classContainingLinkedList.myLinkedListField.First.Value;  // Fixed
        _ = classContainingLinkedList.notALinkedList.First(); // Compliant

        var enumData = new EnumData<int>();
        enumData.First(); // Compliant

        var goodLinkedList = new GoodLinkedList<int>();
        _ = goodLinkedList.First.Value; // Fixed

        var ternary = (true ? data : goodLinkedList).First.Value; // Fixed
        var nullCoalesce = (data ?? goodLinkedList).First.Value;  // Fixed
        var ternaryNullCoalesce = (data ?? (true ? data : goodLinkedList)).First.Value; // Fixed
    }

    void ConditionalsCombinations(GoodLinkedList<int> goodLinkedList)
    {
        _ = goodLinkedList.GetLinkedList().GetLinkedList().GetLinkedList().GetLinkedList().First.Value;     // Fixed
        _ = goodLinkedList.GetLinkedList().GetLinkedList().GetLinkedList().GetLinkedList()?.First.Value;    // Fixed
        _ = goodLinkedList.GetLinkedList().GetLinkedList().GetLinkedList()?.GetLinkedList().First.Value;    // Fixed
        _ = goodLinkedList.GetLinkedList().GetLinkedList().GetLinkedList()?.GetLinkedList()?.First.Value;   // Fixed
        _ = goodLinkedList.GetLinkedList().GetLinkedList()?.GetLinkedList().GetLinkedList().First.Value;    // Fixed
        _ = goodLinkedList.GetLinkedList().GetLinkedList()?.GetLinkedList().GetLinkedList()?.First.Value;   // Fixed
        _ = goodLinkedList.GetLinkedList().GetLinkedList()?.GetLinkedList()?.GetLinkedList().First.Value;   // Fixed
        _ = goodLinkedList.GetLinkedList().GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.First.Value;  // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList().GetLinkedList().GetLinkedList().First.Value;    // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList().GetLinkedList().GetLinkedList()?.First.Value;   // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList().GetLinkedList()?.GetLinkedList().First.Value;   // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList().GetLinkedList()?.GetLinkedList()?.First.Value;  // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList().GetLinkedList().First.Value;   // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList().GetLinkedList()?.First.Value;  // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.GetLinkedList().First.Value;  // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.First.Value; // Fixed
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.First(x => x > 0); // Compliant
    }

    int GetFirst(LinkedList<int> data) => data.First.Value; // Fixed

    class GoodLinkedList<T> : LinkedList<T>
    {
        public GoodLinkedList<T> GetLinkedList() => this;
        T CallFirst() => this.First.Value; // Fixed
    }

    class EnumData<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => null;
        IEnumerator IEnumerable.GetEnumerator() => null;
    }

    class ClassContainingLinkedList
    {
        public LinkedList<int> myLinkedListField = new LinkedList<int>();

        public LinkedList<int> myLinkedListProperty
        {
            get => myLinkedListField;
            set => myLinkedListField.AddLast(value.First.Value); // Fixed
        }

        public NotALinkedList notALinkedList = new NotALinkedList();
    }
}

public class NotALinkedList
{
    public int First() => 0;
}
