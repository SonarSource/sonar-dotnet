using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class TestCases
    {
        void Indexers(Dictionary<int, int> items)
        {
            int index;

            items[index] = 1; // Secondary
//          ^^^^^^^^^^^^^^^^
            items[index] = 2; // Noncompliant {{Verify this is the key that was intended; a value has already been set for it.}}
//          ^^^^^^^^^^^^^^^^

            items[0] = 1; // Secondary
            items[1] = 2;
            items[0] = 3; // Noncompliant

            items[index++] = 1;
            items[index++] = 2; // Compliant, non-const or variable indexes are ignored

            items[2] = 1;
            if (true)
            {
                items[2] = 2; // Compliant, it is conditional
            }

            items[3] = 1; // Secondary
            items[3] = items[5]; // Noncompliant, not the same item is read
        }

        void Methods(Dictionary<int, int> items)
        {
            items.Add(0, 1); // Secondary
            items.Add(0, 2); // Noncompliant
//          ^^^^^^^^^^^^^^^

            items[1] = 1; // Secondary
            items.Add(1, 2); // Noncompliant

            items.Add(2, 2); // Secondary
            items[2] = 1; // Noncompliant
        }

        void Exceptions(Dictionary<int, int> items)
        {
            int index;

            items[index] = 1;
            index++; // any statement that does not contain element access or invocation breaks the search
                     // for elements because currently looked up key is not const and could be modifying it
            items[index] = 2; // Compliant, not subsequent lines

            items[1] = 1; // Secondary
            index++; // this does not break the search for elements because currently looked up key is const
            items[1] = 2; // Noncompliant

            items[2] = 1;
            // existing item is used
            items.Add(2, items[2]);

            items[3] = 1;
            // existing item is read
            items[3] = items[3];
            items[3] = items[3] + 1;
            items[3] = 1 + items[3];
        }

        void Misc(Dictionary<int, int> items1, Dictionary<int, int> items2)
        {
            items1.Add(0, 1);
            items2.Add(0, 2); // Compliant, different objects
        }

        void Arrays(int[] items)
        {
            items[0] = 1; // Secondary
            items[0] = 1; // Noncompliant
        }

        void Lists(List<int> items)
        {
            items[0] = 1; // Secondary
            items[0] = 1; // Noncompliant {{Verify this is the index that was intended; a value has already been set for it.}}
            items.Add(5); // Compliant, this is not overwriting items
        }

        void CustomClass(Custom custom)
        {
            custom["a"] = 1; // Secondary
            custom["a"] = 1; // Noncompliant
        }
    }

    class Custom
    {
        int this[string key]
        {
            get { return 1; }
            set { }
        }
    }
}
