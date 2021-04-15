using System;

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

        public override int GetHashCode() // Fixed
        {
            int hash = Zero;
            hash += foo.GetHashCode();
            hash += age.GetHashCode();
            hash += this.name.GetHashCode();
            hash += name.GetHashCode(); // Compliant, we already reported on this symbol
            hash += this.birthday.GetHashCode();
            hash += SomeMethod(Field);
            return hash;
        }
        public int SomeMethod(int value)
        {
            int hash = Zero;
            hash += this.age.GetHashCode();
            return hash;
        }
    }
}
