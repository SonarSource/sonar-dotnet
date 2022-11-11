using System.Collections.Generic;

namespace MyLibrary
{
    interface IMyInterface
    {
        static virtual string SomeMethod(string s1, string s2) => $"{s1}{s2}";
    }

    class IMyClass : IMyInterface
    {
        static string SomeMethod(string s1, object s2) => "With object"; // Compliant
    }
}
