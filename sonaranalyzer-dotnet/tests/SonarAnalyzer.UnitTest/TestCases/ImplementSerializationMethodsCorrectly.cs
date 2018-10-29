using System;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    [Serializable]
    public class Foo
    {
        [OnSerializing]
        public void OnSerializing(StreamingContext context) { } // Noncompliant {{Make this method 'private'.}}
//                  ^^^^^^^^^^^^^

        [OnSerialized]
        int OnSerialized(StreamingContext context) { return 1; } // Noncompliant {{Make this method return 'void'.}}

        [OnDeserializing]
        void OnDeserializing() { } // Noncompliant  {{Make this method have a single parameter of type 'StreamingContext'.}}

        [OnDeserialized]
        void OnDeserialized(StreamingContext context, string str) { } // Noncompliant {{Make this method have a single parameter of type 'StreamingContext'.}}

        void OnDeserialized2(StreamingContext context, string str) { } // Compliant

        [OnDeserialized]
        void OnDeserialized<T>(StreamingContext context) { } // Noncompliant {{Make this method have no type parameters.}}

        [OnDeserializing]
        public int OnDeserializing2(StreamingContext context) { throw new NotImplementedException(); } // Noncompliant {{Make this method 'private' and return 'void'.}}

        [OnDeserializing]
        public void OnDeserializing3() { throw new NotImplementedException(); } // Noncompliant {{Make this method 'private' and have a single parameter of type 'StreamingContext'.}}

        [OnDeserializing]
        int OnDeserializing4() { throw new NotImplementedException(); } // Noncompliant {{Make this method return 'void' and have a single parameter of type 'StreamingContext'.}}

        [OnDeserializing]
        public int OnDeserializing5<T>() { throw new NotImplementedException(); } // Noncompliant {{Make this method 'private', return 'void', have no type parameters and have a single parameter of type 'StreamingContext'.}}
    }
}
