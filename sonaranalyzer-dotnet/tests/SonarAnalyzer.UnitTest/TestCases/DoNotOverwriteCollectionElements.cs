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
//          ^^^^^^^
            dict[0] = 42; // Noncompliant {{Verify this is the index/key that was intended; a value has already been set for it.}}
//          ^^^^^^^
        }

        void SameIndexOnArray(int[] array)
        {
            array[0] = 42; // Secondary
            array[0] = 42; // Noncompliant
        }

        void SameIndexOnList(List<int> list)
        {
            list[0] = 42; // Secondary
            list[0] = 42; // Noncompliant
        }

        void SameIndexOnArray(CustomIndexerOneArg obj)
        {
            obj["foo"] = 42; // Secondary
            obj["foo"] = 42; // Noncompliant
        }

        void SameIndexOnArray(CustomIndexerMultiArg obj)
        {
            obj["s", 1, 1.0] = 42; // Secondary
            obj["s", 1, 1.0] = 42; // Noncompliant
        }

        void SameIndexSpacedOut(string[] names)
        {
            names["a"] = "a"; // Secondary
            names["b"] = "b";
            names["a"] = "c"; // Noncompliant
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
//          ^^^^^^^^^^^^^^
            dict.Add(0, 1); // Noncompliant
//          ^^^^^^^^^^^^^^
        }

        void DictionaryAdd(Dictionary<int, int> dict)
        {
            dict.Add(0, 0); // Secondary
            dict.Add(0, 1); // Noncompliant
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

        void rspec(IDictionary<string, string> towns)
        {
            towns.Add(y, "Boston");
            towns[y] = "Paris"; // FN - issue #1908
        }
    }

    class InheritanceTest : Dictionary<int, int>
    {
        void InheritanceTest()
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

    class RegressionTests
    {
        // See https://github.com/SonarSource/sonar-csharp/issues/1967
        void NullReference()
        {
            var act = new Action<int, int>((x, y) => x++);
            var dict = new Dictionary<int, int>();

            act(0, 1); // Some invocation that's not a method, but still have two arguments
            dict.Add(0, 0); // Secondary
            dict.Add(0, 1); // Noncompliant
        }
    }

    class CustomIndexerOneArg
    {
        int this[string key]
        {
            get { return 1; }
            set { }
        }
    }

    class CustomIndexerMultiArg
    {
        int this[string s, int i, double d]
        {
            get { return 1; }
            set { }
        }
    }

    class CustomAdd
    {
        public void Add(int a, int b) { }
    }
}
