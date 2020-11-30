using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class TypeExaminationOnSystemType
    {
        public void Test()
        {
            var type = typeof(int);
            var ttype = type.GetType(); //Noncompliant {{Remove this use of 'GetType' on a 'System.Type'.}}
//                          ^^^^^^^^^^

            var s = "abc";

            if (s.GetType().IsInstanceOfType(typeof(string))) //Noncompliant {{Pass an argument that is not a 'System.Type' or consider using 'IsAssignableFrom'.}}
//                                           ^^^^^^^^^^^^^^
            { /* ... */ }

            if (s.GetType().IsInstanceOfType("ssss".GetType())) // Noncompliant {{Consider removing the 'GetType' call, it's suspicious in an 'IsInstanceOfType' call.}}
            { /* ... */ }

            if (s.GetType().IsInstanceOfType(typeof(string) // Noncompliant
                .GetType())) // Noncompliant
            { /* ... */ }

            if (s.GetType().IsInstanceOfType("ssss"))
            { /* ... */ }

            var t = s.GetType();

            var x = Type.GetType("");
        }
    }
}
