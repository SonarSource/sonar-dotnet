using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class TestCases
    {
        public static readonly IDictionary<int, int> dictionaryField;

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

        void PlusPlusIndexAccess(int[] values)
        {
            int index = 0;
            values[index++] = 1; // Secondary
            values[index++] = 2; // Noncompliant - FP - it's not the same index
        }

        void IDictionaryAdd(IDictionary<int, int> dict)
        {
            dict.Add(0, 0); // Secondary
//          ^^^^^^^^^^^^^^
            dict.Add(0, 1); // Noncompliant
//          ^^^^^^^^^^^^^^
        }

        void IDictionaryAddOnMultiMemberAccess(TestCases c)
        {
            c.dictionaryField.Add(0, 0); // Secondary
            c.dictionaryField.Add(0, 1); // Noncompliant
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
}
