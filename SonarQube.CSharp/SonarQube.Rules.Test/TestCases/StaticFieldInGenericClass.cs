using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.TestCases
{
    internal class C
    {
        private static Dictionary<string, List<int>> Dict; 
    }

    class StaticFieldInGenericClass<T/*comment*/, /*comment*/U>
    {
        private static Dictionary<string, List<T>> Dict; 

        public static string sProp1 { get; set; } //Noncompliant
        public /*comment */static string sProp2 { get; set; } //Noncompliant
        public string sProp3 { get; set; }

        public static T tProp { get; set; } //Noncompliant

        internal static string sField; //Noncompliant
    }
}
