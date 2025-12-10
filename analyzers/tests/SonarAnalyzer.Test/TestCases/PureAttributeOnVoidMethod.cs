using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Tests.TestCases
{
    public class MyAttribute : Attribute { }

    class Person
    {
        private int age;

        [Pure] // Noncompliant {{Remove the 'Pure' attribute or change the method to return a value.}}
//       ^^^^
        void ConfigureAge(int age)
        {
            this.age = age;
        }

        [Pure] // Noncompliant
        Task TaskDoesNotReturn(int input)
        {
            return Task.FromResult(input);
        }

        [Pure] // Noncompliant
        async Task AsyncTaskDoesNotReturn(int age)
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

        [Pure]
        void VoidWithRef(ref int age)
        {
            age++;
        }

        [Pure]
        Task<int> TaskOfTReturns(int input)
        {
            return Task.FromResult(input * 42);
        }

        [Pure]
        Task TaskWithOutParameter(int input, out int ret)
        {
            ret = input;
            return Task.FromResult(input);
        }
    }
}


record Record   // Error [CS0246] The type or namespace name 'record' could not be found (are you missing a using directive or an assembly reference?)
                // Error@-1 [CS0548] '<invalid-global-code>.Record': property or indexer must have at least one accessor
{
    Record()    // Error [CS1014] A get or set accessor expected
                // Error@-1 [CS1014] A get or set accessor expected
                // Error@-2 [CS1014] A get or set accessor expected
    {
        [Pure]
        void LocalFunction() { }
    }
}
