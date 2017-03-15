using System;
using System.Collections.Generic;
using System.Runtime.Remoting.Metadata.W3cXsd2001;

namespace Tests.Diagnostics
{
    public class AnyOther
    {
        public readonly int Field;
    }

    public class GetHashCodeMutable : AnyOther
    {
        public readonly DateTime birthday;
        public const int Zero = 0;
        public readonly int age;
        public readonly string name;
        int foo, bar;

        public GetHashCodeMutable()
        {
        }

        public override int GetHashCode()
        {
            int hash = Zero;
            hash += foo.GetHashCode(); // Fixed
            hash += age.GetHashCode(); // Fixed
            hash += this.name.GetHashCode(); // Fixed
            hash += name.GetHashCode(); // Compliant, we already reported on this symbol
            hash += this.birthday.GetHashCode();
            hash += SomeMethod(Field); // Fixed
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
