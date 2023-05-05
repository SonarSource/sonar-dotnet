using System;
using System.Collections.Generic;
using System.Linq;

public class TestClass
{
    bool MyMethod(List<int> list)
    {
        list.Any(x => x > 0); // Noncompliant {{Collection-specific "Exists" method should be used instead of the "Any" extension.}}
//      ^^^^^^^^^^^^^^^^^^^^

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

        return list.Any(x => x % 2 == 0); // Noncompliant
    }

    bool ContainsEvenExpression(List<int> data) =>
        data.Any(x => x % 2 == 0); // Noncompliant

    public class ClassA
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
