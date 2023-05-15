using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TestClass
{
    void MyMethod(List<int> list)
    {
        list.Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
//           ^^^

        list.Append(1).Any(x => x == 1); // Compliant (Appended list becomes an IEnumerable)
        list.Append(1).Append(2).Any(x => x == 1); // Compliant
        list.Append(1).Append(2).Any(x => x == 1).ToString(); // Compliant 

        list.Any(); // Compliant
        list.Contains(0); // Compliant

        var classA = new ClassA();
        classA.myListField.Any(x => x == 0); // Noncompliant
        classA.classB.myListField.Any(x => x == 0); // Noncompliant
        classA.classB.myListField.Any(); // Compliant

        var classB = new ClassB();
        classB.Any(x => x == 0); // Compliant

        list?.Any(x => x == 0); // Noncompliant
        list?.Any(x => x == 0).ToString(); // Noncompliant
        classB?.Any(x => x == 0); // Compliant

        Func<int, bool> del = x => true;
        list.Any(del); // Compliant

        var enumList = new EnumList<int>();
        enumList.Any(x => x == 0); // Compliant

        var goodList = new GoodList<int>();
        goodList.Any(x => x == 0); // Noncompliant

        var ternary = (true ? list : goodList).Any(x => x == 0); // Noncompliant
        var nullCoalesce = (list ?? goodList).Any(x => x == 0); // Noncompliant
        var ternaryNullCoalesce = (list ?? (true ? list : goodList)).Any(x => x == 0); // Noncompliant

        goodList.GetList().Any(x => true); // Compliant

        Any<int>(x => x == 0); // Compliant
        AcceptMethod<int>(goodList.Any); // Compliant
    }

    void List(List<int> list)
    {
        list.Any(x => x == 0); // Noncompliant
        list.Any(x => x > 0); // Compliant
        list.Any(); // Compliant
    }

    void HashSet(HashSet<int> hashSet)
    {
        hashSet.Any(x => x == 0); // Noncompliant
        hashSet.Any(x => x > 0); // Compliant
        hashSet.Any(); // Compliant
    }

    void SortedSet(SortedSet<int> sortedSet)
    {
        sortedSet.Any(x => x == 0); // Noncompliant
        sortedSet.Any(x => x > 0); // Compliant
        sortedSet.Any(); // Compliant
    }

    void Array(int[] array)
    {
        array.Any(x => x == 0); // Compliant
        array.Any(x => x > 0); // Compliant
        array.Any(); // Compliant
    }

    void ConditionalsMatrix(GoodList<int> goodList)
    {
        goodList.GetList().GetList().GetList().GetList().Any(x => x == 0);     // Noncompliant
        goodList.GetList().GetList().GetList().GetList()?.Any(x => x == 0);    // Noncompliant
        goodList.GetList().GetList().GetList()?.GetList().Any(x => x == 0);    // Noncompliant
        goodList.GetList().GetList().GetList()?.GetList()?.Any(x => x == 0);   // Noncompliant
        goodList.GetList().GetList()?.GetList().GetList().Any(x => x == 0);    // Noncompliant
        goodList.GetList().GetList()?.GetList().GetList()?.Any(x => x == 0);   // Noncompliant
        goodList.GetList().GetList()?.GetList()?.GetList().Any(x => x == 0);   // Noncompliant
        goodList.GetList().GetList()?.GetList()?.GetList()?.Any(x => x == 0);  // Noncompliant
        goodList.GetList()?.GetList().GetList().GetList().Any(x => x == 0);    // Noncompliant
        goodList.GetList()?.GetList().GetList().GetList()?.Any(x => x == 0);   // Noncompliant
        goodList.GetList()?.GetList().GetList()?.GetList().Any(x => x == 0);   // Noncompliant
        goodList.GetList()?.GetList().GetList()?.GetList()?.Any(x => x == 0);  // Noncompliant
        goodList.GetList()?.GetList()?.GetList().GetList().Any(x => x == 0);   // Noncompliant
        goodList.GetList()?.GetList()?.GetList().GetList()?.Any(x => x == 0);  // Noncompliant
        goodList.GetList()?.GetList()?.GetList()?.GetList().Any(x => x == 0);  // Noncompliant
        goodList.GetList()?.GetList()?.GetList()?.GetList()?.Any(x => x == 0); // Noncompliant
    }

    void CheckDelegate(
        List<int> intList,
        List<string> stringList,
        List<ClassA> refList,
        int[] intArray,
        string someString,
        int someInt,
        int anotherInt,
        ClassA someRef)
    {
        intList.Any(x => x == 0); // Noncompliant
        intList.Any(x => 0 == x); // Noncompliant
        intList.Any(x => x == someInt); // Noncompliant
        intList.Any(x => someInt == x); // Noncompliant
        intList.Any(x => x.Equals(0));  // Noncompliant
        intList.Any(x => 0.Equals(x));  // Noncompliant
        intList.Any(x => x is 1); // Compliant FN
        intList.Any(x => 1 is x); // Error [CS0150]
        intList.Any(x => x is someInt); // Error [CS0150]
        intList.Any(x => someInt is x); // Error [CS0150]

        intList.Any(x => x == x); // Compliant
        intList.Any(x => someInt == anotherInt); // Compliant
        intList.Any(x => someInt == 0); // Compliant
        intList.Any(x => 0 == 0); // Compliant

        intList.Any(x => x.Equals(x)); // Compliant
        intList.Any(x => someInt.Equals(anotherInt)); // Compliant
        intList.Any(x => someInt.Equals(0)); // Compliant
        intList.Any(x => 0.Equals(0)); // Compliant
        intList.Any(x => x.Equals(x + 1)); // Compliant

        intList.Any(x => x.GetType() == typeof(int)); // Compliant
        intList.Any(x => x.GetType().Equals(typeof(int))); // Compliant
        intList.Any(x => MyIntCheck(x)); // Compliant
        intList.Any(MyIntCheck);  // Compliant
        intList.Any(x => x != 0); // Compliant
        intList.Any(x => x.Equals(0) && true);   // Compliant
        intList.Any(x => (x == 0 ? 2 : 0) == 0); // Compliant
        intList.Any(x => { return x == 0; });    // Compliant

        stringList.Any(x => x == ""); // Noncompliant
        stringList.Any(x => "" == x); // Noncompliant
        stringList.Any(x => x == someString); // Noncompliant
        stringList.Any(x => someString == x); // Noncompliant
        stringList.Any(x => x.Equals("")); // Noncompliant
        stringList.Any(x => "".Equals(x)); // Noncompliant
        stringList.Any(x => Equals(x, "")); // Noncompliant
        stringList.Any(x => x is ""); // Compliant FN
        stringList.Any(x => "" is x); // Error [CS0150]
        stringList.Any(x => x is someString); // Error [CS0150]
        stringList.Any(x => someString is x); // Error [CS0150]

        stringList.Any(x => MyStringCheck(x)); // Compliant
        stringList.Any(MyStringCheck);  // Compliant
        stringList.Any(x => x != "");   // Compliant
        stringList.Any(x => x.Equals("") && true);   // Compliant
        stringList.Any(x => (x == "" ? "a" : "b") == "a"); // Compliant
        stringList.Any(x => x.Equals("" + someString)); // Compliant

        intArray.Any(x => x == 0); // Compliant
        intArray.Any(x => 0 == x); // Compliant
        intArray.Any(x => x == someInt); // Compliant
        intArray.Any(x => someInt == x); // Compliant
        intArray.Any(x => x.Equals(0));  // Compliant
        intArray.Any(x => 0.Equals(x));  // Compliant
        intArray.Any(x => someInt.Equals(x)); // Compliant
        intArray.Any(x => x.Equals(x + 1));   // Compliant

        refList.Any(x => x == someRef); // Compliant
        refList.Any(x => someRef == x); // Compliant
        refList.Any(x => x.Equals(someRef));  // Noncompliant
        refList.Any(x => someRef.Equals(x));  // Noncompliant
        refList.Any(x => Equals(someRef, x)); // Noncompliant
        refList.Any(x => Equals(x, someRef)); // Noncompliant
        refList.Any(x => x is someRef); // Error [CS0150]
        refList.Any(x => someRef is x); // Error [CS0150]

        intList.Any(x => x == null); // Noncompliant FP (warning: the result of this expression will always be false since a value-type is never equal to null)
        intList.Any(x => x.Equals(null));  // Noncompliant FP (warning: the result of this expression will always be false since a value-type is never equal to null)
        intList.Any(x => Equals(x, null)); // Noncompliant FP (warning: the result of this expression will always be false since a value-type is never equal to null)

        refList.Any(x => x == null); // Noncompliant
        refList.Any(x => x.Equals(null));  // Noncompliant
        refList.Any(x => Equals(x, null)); // Noncompliant

        bool MyIntCheck(int x) => x == 0;
        bool MyStringCheck(string x) => x == "";
    }

    void EqualsCheck(List<int> intList, int someInt)
    {
        intList.Any(x => Equals(x, someInt, someInt)); // Compliant

        bool Equals(int a, int b, int c) => false;
    }

    bool ContainsEvenExpression(List<int> data) =>
        data.Any(x => x % 2 == 0); // Compliant

    bool Any<T>(Func<T, bool> predicate) => true;

    void AcceptMethod<T>(Func<Func<T, bool>, bool> methodThatLooksLikeAny) { }

    class GoodList<T> : List<T>
    {
        public GoodList<T> GetList() => this;
        void CallAny() => this.Any(x => x.Equals(0)); // Noncompliant
    }

    class EnumList<T> : IEnumerable<T>
    {
        public IEnumerator<T> GetEnumerator() => null;
        IEnumerator IEnumerable.GetEnumerator() => null;
    }

    class ClassA
    {
        public List<int> myListField = new List<int>();

        public List<int> myListProperty
        {
            get => myListField;
            set
            {
                myListField.AddRange(value);
                var a = myListField.Any(x => x == 0); // Noncompliant
                var b = myListField.Exists(x => x == 0); // Compliant
            }
        }

        public ClassB classB = new ClassB();
    }
}

public class ClassB
{
    public List<int> myListField = new List<int>();

    public bool Any(Func<int, bool> predicate) => false;
}
