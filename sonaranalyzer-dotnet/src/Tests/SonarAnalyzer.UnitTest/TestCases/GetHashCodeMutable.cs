using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Tests.Diagnostics
{
    public class AnyOther
    {
        public int Field;
    }

    public class GetHashCodeMutable : AnyOther
    {
        public readonly DateTime birthday;
        public const int Zero = 0;
        public int age;
        public string name;
        int foo, bar;

        public GetHashCodeMutable()
        {
        }

        public override int GetHashCode()
        {
            int hash = Zero;
            hash += foo.GetHashCode(); // Noncompliant {{Remove this use of 'foo' from the 'GetHashCode' declaration, or make it 'readonly'.}}
//                  ^^^
            hash += age.GetHashCode(); // Noncompliant
            hash += this.name.GetHashCode(); // Noncompliant
            hash += name.GetHashCode(); // Compliant, we already reported on this symbol
            hash += this.birthday.GetHashCode();
            hash += SomeMethod(Field); // Noncompliant
            return hash;
        }
        public int SomeMethod(int value)
        {
            int hash = Zero;
            hash += this.age.GetHashCode();
            return hash;
        }
    }

    class
    {
        int i;
        public override int GetHashCode()
        {
            return i; // we don't report on this
        }
    }
}
