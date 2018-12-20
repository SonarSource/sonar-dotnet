using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class TestCases
    {
        public IDictionary<int, int> dictionaryField;

        void SameIndexOnDictionary(Dictionary<int, int> dict)
        {
            dict[0] = 42; // Secondary
//          ^^^^^^^^^^^^^
            dict[0] = 42; // Noncompliant {{Verify this is the index/key that was intended; a value has already been set for it.}}
//          ^^^^^^^^^^^^^
        }

        void SameIndexOnArray(int[] array)
        {
            array[0] = 42; // Secondary
            array[0] = 42; // Noncompliant
        }

        void Parenthesis_Indexer(int[] array)
        {
            array[(0)] = 42; // Secondary
            (array)[0] = 42; // Noncompliant
        }

        void Parenthesis_Invocation(Dictionary<int, int> dict)
        {
            dict.Add((0), 42); // Secondary
            (dict).Add(0, 42); // Noncompliant
        }

        void SameIndexOnList(List<int> list)
        {
            list[0] = 42; // Secondary
            list[0] = 42; // Noncompliant
        }

        void SameIndexOnArray(CustomIndexerOneArg obj)
        {
            obj["foo"] = 42; // Compliant, not a collection or dictionary
            obj["foo"] = 42; // Compliant, not a collection or dictionary
        }

        void SameIndexOnArray(CustomIndexerMultiArg obj)
        {
            obj["s", 1, 1.0] = 42; // Compliant, not a collection or dictionary
            obj["s", 1, 1.0] = 42; // Compliant, not a collection or dictionary
        }

        void SameIndexSpacedOut(string[] names)
        {
            names[0] = "a"; // Secondary
            names[1] = "b";
            names[0] = "c"; // Noncompliant
        }

        void NonSequentialAccessOnSameIndex(int[] values)
        {
            int index = 0;
            values[0] = 1;
            index++;
            values[0] = 2; // FN - We only take consecutive element access
        }

        void NonConstantConsecutiveIndexAccess(int[] values)
        {
            int index = 0;
            values[index] = 1; // Secondary
            values[index] = 2; // Noncompliant
        }

        void IncrementDecrementIndexAccess(int[] values)
        {
            int index = 0;
            values[index++] = 1;
            values[index++] = 2;

            values[index--] = 1;
            values[index--] = 2;

            values[++index] = 1;
            values[++index] = 2;

            values[--index] = 1;
            values[--index] = 2;
        }

        void IDictionaryAdd(IDictionary<int, int> dict)
        {
            dict.Add(0, 0); // Secondary
//          ^^^^^^^^^^^^^^^
            dict.Add(0, 1); // Noncompliant
//          ^^^^^^^^^^^^^^^
        }

        void DictionaryAdd(Dictionary<int, int> dict)
        {
            dict.Add(0, 0); // Secondary
            dict.Add(0, 1); // Noncompliant
        }

        void ListRemove(List<int> list)
        {
            list.Remove(0);
            list.Remove(0); // Ignore methods that do not add/set items
            list[0] = 1; // Compliant
        }

        void IDictionaryAddOnMultiMemberAccess(TestCases c)
        {
            c.dictionaryField.Add(0, 0); // Secondary
            c.dictionaryField.Add(0, 1); // Noncompliant
        }

        void IDictionaryAddWithThis()
        {
            this.dictionaryField.Add(0, 0); // Secondary
            this.dictionaryField.Add(0, 1); // Noncompliant
        }

        void DoNotReportOnNonDictionaryAdd(CustomAdd c)
        {
            c.Add(0, 1);
            c.Add(0, 2); // Compliant this is not on a dictionary
        }

        void AccessOnMethodCall()
        {
            GetArray()[0] = 1;
            GetArray()[0] = 2;
        }

        int[] GetArray()
        {
            return new int[1];
        }

        void InitTowns(IDictionary<string, string> towns, string y)
        {
            towns.Add(y, "Boston"); // Secondary
            towns[y] = "Paris"; // Noncompliant, https://github.com/SonarSource/sonar-csharp/issues/1908
        }

        void MemberBinding(IDictionary<string, string> dictionary)
        {
            dictionary?.Add("a", "b"); // Secondary
            dictionary?.Add("a", "b"); // Noncompliant
        }
    }

    class InheritanceTest : Dictionary<int, int>
    {
        void AddToBaseField()
        {
            base.Add(0, 0); // Secondary
            base.Add(0, 1); // Noncompliant
        }

        void MyAdd()
        {
            this.Add(0, 0); // Secondary
            this.Add(0, 1); // Noncompliant
        }

        void IncrementDecrementInvocation(Dictionary<int, int> dict)
        {
            int index = 0;
            dict.Add(index++, 1);
            dict.Add(index++, 2);

            dict.Add(index--, 1);
            dict.Add(index--, 2);

            dict.Add(++index, 1);
            dict.Add(++index, 2);

            dict.Add(--index, 1);
            dict.Add(--index, 2);
        }
    }

    public class RegressionTests
    {
        private RegressionTests instance1;
        private RegressionTests instance2;
        private RegressionTests instance3;

        public Dictionary<string, string> Map { get; }

        // See https://github.com/SonarSource/sonar-csharp/issues/1967
        public void NullReference()
        {
            var act = new Action<int, int>((x, y) => x++);
            var dict = new Dictionary<int, int>();

            act(0, 1); // Some invocation that's not a method, but still have two arguments
            dict.Add(0, 0); // Secondary
            dict.Add(0, 1); // Noncompliant
        }

        public void Foo()
        {
            instance1.Map.Add("currentColor", "#FFFF0000");
            instance2.Map.Add("currentColor", "#FF00FF00");
        }

        public void Bar()
        {
            Map.Add("currentColor", "#FFFF0000"); // Secondary
            Map.Add("currentColor", "#FF00FF00"); // Noncompliant
        }

        public void FooBar()
        {
            MyTestClass1.Map.Add("currentColor", "#FFFF0000");
            MyTestClass2.Map.Add("currentColor", "#FF00FF00");
        }
    }

    public static class MyTestClass1
    {
        public static Dictionary<string, string> Map { get; }
    }

    public static class MyTestClass2
    {
        public static Dictionary<string, string> Map { get; }
    }

    public class CustomIndexerOneArg
    {
        public int this[string key]
        {
            get { return 1; }
            set { }
        }
    }

    public class CustomIndexerMultiArg
    {
        public int this[string s, int i, double d]
        {
            get { return 1; }
            set { }
        }
    }

    public class CustomAdd
    {
        public void Add(int a, int b) { }
    }
}
