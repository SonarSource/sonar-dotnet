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

            typeof(Type).GetType(); // Compliant - can be used by convention to get an instance of ‘System.RuntimeType’. See: https://github.com/SonarSource/sonar-dotnet/issues/4201
            typeof(System.Type).GetType();
            typeof(object).GetType(); // Noncompliant - only `typeof(Type)` is considered an exception

            GetType(string.Empty); // Compliant - different `GetType` method
            IsInstanceOfType(string.Empty); // Compliant - different `IsInstanceOfType` method
        }

        public Type ResolveType(string id) => Type.GetType(id);

        public void GetType(object o) { }

        public void IsInstanceOfType(object o) { }
    }
}
