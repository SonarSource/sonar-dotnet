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

            _ = collection.Select([OnSerialized] (x) => x + 1);  // Compliant - FN

            Action a = [OnDeserializing] () => { }; // Compliant - FN

            Action x = true
                           ? ([OnDeserialized] () => { }) // Compliant - FN
                           :[GenericAttribute<int>] () => { };

            Call([GenericAttribute<int>] (x) => { });
        }

        private void Call(Action<int> action) => action(1);
    }
    public class NonGenericAttribute : Attribute { }

    public class GenericAttribute<T> : Attribute { }
}
