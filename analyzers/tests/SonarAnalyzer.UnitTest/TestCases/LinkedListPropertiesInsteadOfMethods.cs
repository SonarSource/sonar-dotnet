using System.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

class MyClass
{
    void Basic(LinkedList<int> data)
    {
        _ = data.First(); // Noncompliant {{'First' property of 'LinkedList' should be used instead of the 'First()' extension method.}}
//               ^^^^^
        _ = data.Last(); // Noncompliant {{'Last' property of 'LinkedList' should be used instead of the 'Last()' extension method.}}
//               ^^^^
        data.First(x => x > 0);            // Compliant
        data.Last(x => x > 0);             // Compliant
        _ = data.First.Value;              // Compliant
        _ = data.Last.Value;               // Compliant
        data.Count();                      // Compliant
        data.Append(1).First().ToString(); // Compliant
        data.Append(1).Last().ToString();  // Compliant
        data?.First().ToString();          // Noncompliant
        data?.Last().ToString();           // Noncompliant

    }

    void CustomClass(LinkedList<int> data)
    {
        var classContainingLinkedList = new ClassContainingLinkedList();
        _ = classContainingLinkedList.myLinkedListField.First();  // Noncompliant
        _ = classContainingLinkedList.notALinkedList.First(); // Compliant

        var enumData = new EnumData<int>();
        enumData.First(); // Compliant

        var goodLinkedList = new GoodLinkedList<int>();
        _ = goodLinkedList.First(); // Noncompliant

        var ternary = (true ? data : goodLinkedList).First(); // Noncompliant
        var nullCoalesce = (data ?? goodLinkedList).First();  // Noncompliant
        var ternaryNullCoalesce = (data ?? (true ? data : goodLinkedList)).First(); // Noncompliant
    }

    void ConditionalsCombinations(GoodLinkedList<int> goodLinkedList)
    {
        _ = goodLinkedList.GetLinkedList().GetLinkedList().GetLinkedList().GetLinkedList().First();     // Noncompliant
        _ = goodLinkedList.GetLinkedList().GetLinkedList().GetLinkedList().GetLinkedList()?.First();    // Noncompliant
        _ = goodLinkedList.GetLinkedList().GetLinkedList().GetLinkedList()?.GetLinkedList().First();    // Noncompliant
        _ = goodLinkedList.GetLinkedList().GetLinkedList().GetLinkedList()?.GetLinkedList()?.First();   // Noncompliant
        _ = goodLinkedList.GetLinkedList().GetLinkedList()?.GetLinkedList().GetLinkedList().First();    // Noncompliant
        _ = goodLinkedList.GetLinkedList().GetLinkedList()?.GetLinkedList().GetLinkedList()?.First();   // Noncompliant
        _ = goodLinkedList.GetLinkedList().GetLinkedList()?.GetLinkedList()?.GetLinkedList().First();   // Noncompliant
        _ = goodLinkedList.GetLinkedList().GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.First();  // Noncompliant
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList().GetLinkedList().GetLinkedList().First();    // Noncompliant
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList().GetLinkedList().GetLinkedList()?.First();   // Noncompliant
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList().GetLinkedList()?.GetLinkedList().First();   // Noncompliant
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList().GetLinkedList()?.GetLinkedList()?.First();  // Noncompliant
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList().GetLinkedList().First();   // Noncompliant
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList().GetLinkedList()?.First();  // Noncompliant
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.GetLinkedList().First();  // Noncompliant
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.First(); // Noncompliant
//                                                                                             ^^^^^
        _ = goodLinkedList.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.GetLinkedList()?.First(x => x > 0); // Compliant
    }

    int GetFirst(LinkedList<int> data) => data.First(); // Noncompliant

    class GoodLinkedList<T> : LinkedList<T>
    {
        public GoodLinkedList<T> GetLinkedList() => this;
        T CallFirst() => this.First(); // Noncompliant
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
            set => myLinkedListField.AddLast(value.First()); // Noncompliant
        }

        public NotALinkedList notALinkedList = new NotALinkedList();
    }
}

public class NotALinkedList
{
    public int First() => 0;
}
