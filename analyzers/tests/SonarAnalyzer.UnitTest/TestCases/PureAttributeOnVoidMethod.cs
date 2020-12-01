using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Tests.TestCases
{
    public class MyAttribute : Attribute
    {
    }

    class Person
    {
        private int age;
        [Pure] // Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
//       ^^^^
        void ConfigureAge(int age)
        {
            this.age = age;
        }

        [My]
        void ConfigureAge2(int age)
        {
            this.age = age;
        }

        [Pure]
        int ConfigureAge3(int age)
        {
            return age;
        }

        [Pure]
        void ConfigureAge4(int age, out int ret)
        {
            ret = age;
        }
    }
}
