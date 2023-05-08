using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TestClass
{
    bool MyMethod(List<int> list)
    {
        list.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
//           ^^^

        list.Append(1).Any(x => x > 1); // Compliant (Appended list becomes an IEnumerable)
        list.Append(1).Append(2).Any(x => x > 1); // Compliant
        list.Append(1).Append(2).Any(x => x > 1).ToString(); // Compliant 

        list.Any(); // Compliant (you can't use Exists with no arguments, CS7036)
        list.Exists(x => x > 0); // Compliant

        var classA = new ClassA();
        classA.myListField.Any(x => x > 0); // Noncompliant
        classA.classB.myListField.Any(x => x > 0); // Noncompliant
        classA.classB.myListField.Any(); // Compliant

        var classB = new ClassB();
        classB.Any(x => x); // Compliant

        var boolList = new List<bool>();

        list?.Any(x => x > 0); // Noncompliant
        list?.Any(x => x > 0).ToString(); // Noncompliant
        classB?.Any(x => x); // Compliant

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

        goodList.GetList().GetList().GetList().GetList().Any(x => x > 0);     //Noncompliant
        goodList.GetList().GetList().GetList().GetList()?.Any(x => x > 0);    //Noncompliant
        goodList.GetList().GetList().GetList()?.GetList().Any(x => x > 0);    //Noncompliant
        goodList.GetList().GetList().GetList()?.GetList()?.Any(x => x > 0);   //Noncompliant
        goodList.GetList().GetList()?.GetList().GetList().Any(x => x > 0);    //Noncompliant
        goodList.GetList().GetList()?.GetList().GetList()?.Any(x => x > 0);   //Noncompliant
        goodList.GetList().GetList()?.GetList()?.GetList().Any(x => x > 0);   //Noncompliant
        goodList.GetList().GetList()?.GetList()?.GetList()?.Any(x => x > 0);  //Noncompliant
        goodList.GetList()?.GetList().GetList().GetList().Any(x => x > 0);    //Noncompliant
        goodList.GetList()?.GetList().GetList().GetList()?.Any(x => x > 0);   //Noncompliant
        goodList.GetList()?.GetList().GetList()?.GetList().Any(x => x > 0);   //Noncompliant
        goodList.GetList()?.GetList().GetList()?.GetList()?.Any(x => x > 0);  //Noncompliant
        goodList.GetList()?.GetList()?.GetList().GetList().Any(x => x > 0);   //Noncompliant
        goodList.GetList()?.GetList()?.GetList().GetList()?.Any(x => x > 0);  //Noncompliant
        goodList.GetList()?.GetList()?.GetList()?.GetList().Any(x => x > 0);  //Noncompliant
        goodList.GetList()?.GetList()?.GetList()?.GetList()?.Any(x => x > 0); //Noncompliant

        return list.Any(x => x % 2 == 0); // Noncompliant
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

    public bool Any(Func<bool, bool> predicate) => false;
}
