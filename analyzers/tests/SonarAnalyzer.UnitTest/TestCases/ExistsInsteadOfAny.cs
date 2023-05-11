using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TestClass
{
    void MyMethod(List<int> list, int[] array)
    {
        list.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
//           ^^^

        list.Append(1).Any(x => x > 1); // Compliant (Appended list becomes an IEnumerable)
        list.Append(1).Append(2).Any(x => x > 1); // Compliant
        list.Append(1).Append(2).Any(x => x > 1).ToString(); // Compliant 

        list.Any(); // Compliant (you can't use Exists with no arguments, CS7036)
        list.Exists(x => x > 0); // Compliant

        array.Any(x => x > 0); // Noncompliant
        array.Any(); // Compliant

        var classA = new ClassA();
        classA.myListField.Any(x => x > 0); // Noncompliant
        classA.classB.myListField.Any(x => x > 0); // Noncompliant
        classA.classB.myListField.Any(); // Compliant

        var classB = new ClassB();
        classB.Any(x => x > 0); // Compliant

        list?.Any(x => x > 0); // Noncompliant
        list?.Any(x => x > 0).ToString(); // Noncompliant
        classB?.Any(x => x > 0); // Compliant

        Func<int, bool> del = x => true;
        list.Any(del); // Noncompliant

        var enumList = new EnumList<int>();
        enumList.Any(x => x > 0); // Compliant

        var goodList = new GoodList<int>();
        goodList.Any(x => x > 0); // Noncompliant

        var ternary = (true ? list : goodList).Any(x => x > 0); // Noncompliant
        var nullCoalesce = (list ?? goodList).Any(x => x > 0); // Noncompliant
        var ternaryNullCoalesce = (list ?? (true ? list : goodList)).Any(x => x > 0); // Noncompliant

        goodList.GetList().Any(x => true); // Noncompliant

        Any<int>(x => x > 0); // Compliant
        AcceptMethod<int>(goodList.Any); // Compliant
    }

    void ConditionalsMatrix(GoodList<int> goodList)
    {
        goodList.GetList().GetList().GetList().GetList().Any(x => x > 0);     // Noncompliant
        goodList.GetList().GetList().GetList().GetList()?.Any(x => x > 0);    // Noncompliant
        goodList.GetList().GetList().GetList()?.GetList().Any(x => x > 0);    // Noncompliant
        goodList.GetList().GetList().GetList()?.GetList()?.Any(x => x > 0);   // Noncompliant
        goodList.GetList().GetList()?.GetList().GetList().Any(x => x > 0);    // Noncompliant
        goodList.GetList().GetList()?.GetList().GetList()?.Any(x => x > 0);   // Noncompliant
        goodList.GetList().GetList()?.GetList()?.GetList().Any(x => x > 0);   // Noncompliant
        goodList.GetList().GetList()?.GetList()?.GetList()?.Any(x => x > 0);  // Noncompliant
        goodList.GetList()?.GetList().GetList().GetList().Any(x => x > 0);    // Noncompliant
        goodList.GetList()?.GetList().GetList().GetList()?.Any(x => x > 0);   // Noncompliant
        goodList.GetList()?.GetList().GetList()?.GetList().Any(x => x > 0);   // Noncompliant
        goodList.GetList()?.GetList().GetList()?.GetList()?.Any(x => x > 0);  // Noncompliant
        goodList.GetList()?.GetList()?.GetList().GetList().Any(x => x > 0);   // Noncompliant
        goodList.GetList()?.GetList()?.GetList().GetList()?.Any(x => x > 0);  // Noncompliant
        goodList.GetList()?.GetList()?.GetList()?.GetList().Any(x => x > 0);  // Noncompliant
        goodList.GetList()?.GetList()?.GetList()?.GetList()?.Any(x => x > 0); // Noncompliant
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
        intList.Any(x => x == 0); // Compliant (should raise S6617)
        intList.Any(x => 0 == x); // Compliant (should raise S6617)
        intList.Any(x => x == someInt); // Compliant (should raise S6617)
        intList.Any(x => someInt == x); // Compliant (should raise S6617)
        intList.Any(x => x.Equals(0)); // Compliant (should raise S6617)
        intList.Any(x => 0.Equals(x)); // Compliant (should raise S6617)
        intList.Any(x => x is 1); // Noncompliant FP (should raise S6617)
        intList.Any(x => 1 is x); // Error [CS0150]
        intList.Any(x => x is someInt); // Error [CS0150]
        intList.Any(x => someInt is x); // Error [CS0150]

        intList.Any(x => x == x); // Noncompliant
        intList.Any(x => someInt == anotherInt); // Noncompliant
        intList.Any(x => someInt == 0); // Noncompliant
        intList.Any(x => 0 == 0); // Noncompliant

        intList.Any(x => x.Equals(x)); // Noncompliant
        intList.Any(x => someInt.Equals(anotherInt)); // Noncompliant
        intList.Any(x => someInt.Equals(0)); // Noncompliant
        intList.Any(x => 0.Equals(0)); // Noncompliant
        intList.Any(x => x.Equals(x + 1)); // Noncompliant

        intList.Any(x => x.GetType() == typeof(int)); // Noncompliant
        intList.Any(x => x.GetType().Equals(typeof(int))); // Noncompliant FP
        intList.Any(x => MyIntCheck(x)); // Noncompliant
        intList.Any(x => x != 0);     // Noncompliant
        intList.Any(x => x.Equals(0) && true);   // Noncompliant
        intList.Any(x => (x == 0 ? 2 : 0) == 0); // Noncompliant
        intList.Any(x => { return x == 0; }); // Noncompliant FP

        stringList.Any(x => x == ""); // Compliant (should raise S6617)
        stringList.Any(x => "" == x); // Compliant (should raise S6617)
        stringList.Any(x => x == someString); // Compliant (should raise S6617)
        stringList.Any(x => someString == x); // Compliant (should raise S6617)
        stringList.Any(x => x.Equals("")); // Compliant (should raise S6617)
        stringList.Any(x => "".Equals(x)); // Compliant (should raise S6617)
        stringList.Any(x => Equals(x, "")); // Compliant (should raise S6617)
        stringList.Any(x => x is ""); // Noncompliant FP (should raise S6617)
        stringList.Any(x => "" is x); // Error [CS0150]
        stringList.Any(x => x is someString); // Error [CS0150]
        stringList.Any(x => someString is x); // Error [CS0150]

        stringList.Any(x => MyStringCheck(x)); // Noncompliant
        stringList.Any(x => x != "");     // Noncompliant
        stringList.Any(x => x.Equals("") && true);   // Noncompliant
        stringList.Any(x => (x == "" ? "a" : "b") == "a"); // Noncompliant
        stringList.Any(x => x.Equals("" + someString)); // Noncompliant

        intArray.Any(x => x == 0); // Noncompliant (this is not raising S6617)
        intArray.Any(x => 0 == x); // Noncompliant (this is not raising S6617)
        intArray.Any(x => x == someInt); // Noncompliant (this is not raising S6617)
        intArray.Any(x => someInt == x); // Noncompliant (this is not raising S6617)
        intArray.Any(x => x.Equals(0)); // Noncompliant (this is not raising S6617)
        intArray.Any(x => 0.Equals(x)); // Noncompliant (this is not raising S6617)
        intArray.Any(x => someInt.Equals(x)); // Noncompliant (this is not raising S6617)
        intArray.Any(x => x.Equals(x + 1)); // Noncompliant (this is not raising S6617)

        refList.Any(x => x == someRef); // Noncompliant (this is not raising S6617)
        refList.Any(x => someRef == x); // Noncompliant (this is not raising S6617)
        refList.Any(x => x.Equals(someRef)); // Compliant (should raise S6617)
        refList.Any(x => someRef.Equals(x)); // Compliant (should raise S6617)
        refList.Any(x => Equals(someRef, x)); // Compliant (should raise S6617)
        refList.Any(x => Equals(x, someRef)); // Compliant (should raise S6617)
        refList.Any(x => x is someRef); // Error [CS0150]
        refList.Any(x => someRef is x); // Error [CS0150]

        intList.Any(x => x == null); // Compliant FN (warning: the result of this expression will always be false since a value-type is never equal to null)
        intList.Any(x => x.Equals(null)); // Compliant FN (warning: the result of this expression will always be false since a value-type is never equal to null)
        intList.Any(x => Equals(x, null)); // Compliant FN (warning: the result of this expression will always be false since a value-type is never equal to null)

        refList.Any(x => x == null); // Compliant (should raise S6617)
        refList.Any(x => x.Equals(null)); // Compliant (should raise S6617)
        refList.Any(x => Equals(x, null)); // Compliant (should raise S6617)

        bool MyIntCheck(int x) => x == 0;
        bool MyStringCheck(string x) => x == "";
    }

    void EqualsCheck(List<int> intList, int someInt)
    {
        intList.Any(x => Equals(x, someInt, someInt)); // Noncompliant

        bool Equals(int a, int b, int c) => false;
    }

    bool ContainsEvenExpression(List<int> data) =>
        data.Any(x => x % 2 == 0); // Noncompliant

    bool Any<T>(Func<T, bool> predicate) => true;

    void AcceptMethod<T>(Func<Func<T, bool>, bool> methodThatLooksLikeAny) { }

    class GoodList<T> : List<T>
    {
        public GoodList<T> GetList() => this;
        void CallAny() => this.Any(x => true); // Noncompliant
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
                var b = myListField.Any(x => x > 0); // Noncompliant
                var c = myListField.Exists(x => x > 0); // Compliant
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
