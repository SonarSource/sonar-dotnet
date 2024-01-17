using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace Net6Poc.ImplementSerializationMethodsCorrectly
{
    internal class TestCases
    {
        public void Bar(IEnumerable<int> collection)
        {
            [OnSerializing] int Get() => 1; // Noncompliant {{Serialization attributes on local functions are not considered.}}

            _ = collection.Select([OnSerialized] (x) => x + 1);  // Noncompliant {{Serialization attributes on lambdas are not considered.}}

            Action a = [OnDeserializing] () => { }; // Noncompliant

            Action x = true
                           ? ([OnDeserialized] () => { }) // Noncompliant
                           : [OnDeserialized] () => { };  // Noncompliant

            Call([OnDeserialized] (x) => { }); // Noncompliant
        }

        private void Call(Action<int> action) => action(1);
    }
}
