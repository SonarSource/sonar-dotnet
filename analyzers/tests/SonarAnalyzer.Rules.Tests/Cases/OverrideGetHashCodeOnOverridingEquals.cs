using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    interface IInterface
    {

    }

    class OverrideOnlyEqualsClass // Noncompliant {{This class overrides 'Equals' and should therefore also override 'GetHashCode'.}}
//        ^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }

    class OverrideOnlyGetHashCodeClass // Noncompliant {{This class overrides 'GetHashCode' and should therefore also override 'Equals'.}}
//        ^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    class ValidClass
    {
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    struct OverrideOnlyEqualsStruct // Noncompliant {{This struct overrides 'Equals' and should therefore also override 'GetHashCode'.}}
//         ^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
    }

    struct OverrideOnlyGetHashCodeStruct // Noncompliant {{This struct overrides 'GetHashCode' and should therefore also override 'Equals'.}}
//         ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    struct ValidStruct
    {
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
