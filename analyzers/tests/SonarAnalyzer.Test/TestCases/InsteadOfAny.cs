using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

public class TestClass
{
    void MyMethod(List<int> list, int[] array)
    {
        list.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        //   ^^^
        list.Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        //   ^^^

        list.Append(1).Any(x => x > 1); // Compliant (Appended list becomes an IEnumerable)
        list.Append(1).Append(2).Any(x => x > 1); // Compliant
        list.Append(1).Append(2).Any(x => x > 1).ToString(); // Compliant

        list.Any(); // Compliant (you can't use Exists with no arguments, CS7036)
        list.Exists(x => x > 0); // Compliant

        array.Any(x => x > 0); // Noncompliant
        array.Any(); // Compliant

        var classA = new ClassA();
        classA.myListField.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        classA.classB.myListField.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        classA.classB.myListField.Any(); // Compliant

        var classB = new ClassB();
        classB.Any(x => x > 0); // Compliant

        list?.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        list?.Any(x => x > 0).ToString(); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        classB?.Any(x => x > 0); // Compliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        Func<int, bool> del = x => true;
        list.Any(del); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        var enumList = new EnumList<int>();
        enumList.Any(x => x > 0); // Compliant

        var goodList = new GoodList<int>();
        goodList.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        var ternaryExists = (true ? list : goodList).Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        var nullCoalesceExists = (list ?? goodList).Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        var ternaryNullCoalesceExists = (list ?? (true ? list : goodList)).Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        var ternaryContains = (true ? list : goodList).Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        var nullCoalesceContains = (list ?? goodList).Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        var ternaryNullCoalesceContains = (list ?? (true ? list : goodList)).Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}

        goodList.GetList().Any(x => true); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        Any<int>(x => x > 0); // Compliant
        AcceptMethod<int>(goodList.Any); // Compliant
    }

    void List(List<int> list)
    {
        list.Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        list.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        list.Any(); // Compliant
    }

    void HashSet(HashSet<int> hashSet)
    {
        hashSet.Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        hashSet.Any(x => x > 0); // Compliant
        hashSet.Any(); // Compliant
    }

    void SortedSet(SortedSet<int> sortedSet)
    {
        sortedSet.Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        sortedSet.Any(x => x > 0); // Compliant
        sortedSet.Any(); // Compliant
    }

    void Array(int[] array)
    {
        array.Any(x => x == 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        array.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        array.Any(); // Compliant
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
        intList.Any(x => x == 0); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(x => 0 == x); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(x => x == someInt); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(x => someInt == x); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(x => x.Equals(0));  // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(x => 0.Equals(x));  // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        intList.Any(x => x is 1); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}} Should be Contains
        intList.Any(x => 1 is x); // Error [CS9135]
        intList.Any(x => x is someInt); // Error [CS9135]
        intList.Any(x => someInt is x); // Error [CS9135]

        intList.Any(x => x == x); // Noncompliant
        intList.Any(x => someInt == anotherInt); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => someInt == 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => 0 == 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        intList.Any(x => x.Equals(x)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => someInt.Equals(anotherInt)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => someInt.Equals(0)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => 0.Equals(0)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => x.Equals(x + 1)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        intList.Any(x => x.GetType() == typeof(int)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => x.GetType().Equals(typeof(int))); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}} FP
        intList.Any(x => MyIntCheck(x)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(MyIntCheck);  // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => x != 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => x.Equals(0) && true);   // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => (x == 0 ? 2 : 0) == 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intList.Any(x => { return x == 0; });    // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}} FP

        stringList.Any(x => x == ""); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(x => "" == x); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(x => x == someString); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(x => someString == x); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(x => x.Equals("")); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(x => "".Equals(x)); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(x => Equals(x, "")); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        stringList.Any(x => x is ""); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}} FP
        stringList.Any(x => x is null); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}} Should raise Contains
        stringList.Any(x => "" is x); // Error [CS9135]
        stringList.Any(x => x is someString); // Error [CS9135]
        stringList.Any(x => someString is x); // Error [CS9135]

        stringList.Any(x => MyStringCheck(x)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(MyStringCheck);  // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(x => x != "");   // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(x => x.Equals("") && true);   // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(x => (x == "" ? "a" : "b") == "a"); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        stringList.Any(x => x.Equals("" + someString)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        intArray.Any(x => x == 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(x => 0 == x); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(x => x == someInt); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(x => someInt == x); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(x => x.Equals(0));  // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(x => 0.Equals(x));  // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(x => someInt.Equals(x)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        intArray.Any(x => x.Equals(x + 1));   // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        refList.Any(x => x == someRef); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        refList.Any(x => someRef == x); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
        refList.Any(x => x.Equals(someRef));  // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(x => someRef.Equals(x));  // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(x => Equals(someRef, x)); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(x => Equals(x, someRef)); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(x => x is someRef); // Error [CS9135]
        refList.Any(x => someRef is x); // Error [CS9135]

        intList.Any(x => x == null); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}} (warning: the result of this expression will always be false since a value-type is never equal to null)
        intList.Any(x => x.Equals(null));  // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}} (warning: the result of this expression will always be false since a value-type is never equal to null)
        intList.Any(x => Equals(x, null)); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}} (warning: the result of this expression will always be false since a value-type is never equal to null)

        refList.Any(x => x == null); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(x => x.Equals(null));  // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}
        refList.Any(x => Equals(x, null)); // Noncompliant {{Collection-specific "Contains" method should be used instead of the "Any" extension.}}

        bool MyIntCheck(int x) => x == 0;
        bool MyStringCheck(string x) => x == "";
    }

    void CustomEqualsCheckOneParam(List<int> intList, int someInt)
    {
        intList.Any(x => Equals(x)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        bool Equals(int a) => false;
    }

    void CustomEqualsCheckTwoParam(List<int> intList, int someInt)
    {
        intList.Any(x => Equals(x, someInt)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        bool Equals(int a, int b) => false;
    }

    void CustomEqualsCheckThreeParam(List<int> intList, int someInt)
    {
        intList.Any(x => Equals(x, someInt, someInt)); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

        bool Equals(int a, int b, int c) => false;
    }

    bool ContainsEvenExpression(List<int> data) =>
        data.Any(x => x % 2 == 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}

    bool Any<T>(Func<T, bool> predicate) => true;

    void AcceptMethod<T>(Func<Func<T, bool>, bool> methodThatLooksLikeAny) { }

    class GoodList<T> : List<T>
    {
        public GoodList<T> GetList() => this;
        void CallAny() => this.Any(x => true); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
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
                var b = myListField.Any(x => x > 0);    // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
                var c = myListField.Exists(x => x > 0); // Compliant
                var d = myListField.Contains(0);        // Compliant
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

public class ExpressionTree
{
    // https://github.com/SonarSource/sonar-dotnet/issues/7508
    public void Repro_7508()
    {
        Expression<Func<List<int>, bool>> containsThree = list => list.Any(el => el == 3); // Compliant (IsInExpressionTree)
    }

    class Customer
    {
        public ICollection<Order> Orders { get; } // Orders is an IEnumerable<T> and not IQueryable<T>
    }
    class Order
    {
        public int Id { get; }
    }

    private void NestedQuery(IQueryable<Customer> customers) // typically a DbSet<Customer> https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.dbset-1
    {
        // Typical query in EF (https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many)

        // (order => order.Id > 0) is not an Expression<Func<..>> nor is Orders an IQueryable<T>
        // But the surrounding "customers.Where" are and so we do not raise.
        var qry1 = customers.Where(customer => customer.Orders.Any(order => order.Id > 0)); // Compliant. In expression tree context

        var qry2 = from customer in customers
                   where customer.Orders.Any(order => order.Id > 0) // Compliant. In expression tree context
                   select customer;
    }
}
