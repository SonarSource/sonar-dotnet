using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{

    class GetHashCodeEqualsOverride
    {
        private readonly int x;
        public GetHashCodeEqualsOverride(int x)
        {
            this.x = x;
        }

        public bool Somemethod()
        {
            object obj = null;
            if (base.Equals(obj)) // Compliant
            {
                return true;
            }
            if (base.Equals(obj)) // Compliant
            {
                return false;
            }

            return true;
        }

        public override int GetHashCode()
        {
            return x.GetHashCode() ^ base.GetHashCode(); //Noncompliant {{Remove this 'base' call to 'object.GetHashCode', which is directly based on the object reference.}}
//                                   ^^^^^^^^^^^^^^^^^^
        }
        public override bool Equals(object obj)
        {
            if (base.Equals(obj)) //Compliant, guard condition
            {
                return true;
            }

            if (base.Equals(obj)) //Compliant, guard condition
            {
                return true;
                return true;
            }

            if (base.Equals(obj)) //Noncompliant
            {
                return false;
            }

            if (true)
            {
                return base.Equals(obj); //Compliant
            }

            return base.Equals(obj); //Compliant
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    class GetHashCodeEqualsOverride1
    {
        private readonly int x;
        public GetHashCodeEqualsOverride1(int x)
        {
            this.x = x;
        }

        public override bool Equals(object obj)
        {
            if (true)
            {
                return base.Equals(obj); //Noncompliant
            }

            return base.Equals(obj); // Compliant
        }
    }

    class GetHashCodeEqualsOverride2
    {
        private readonly int x;
        public GetHashCodeEqualsOverride2(int x)
        {
            this.x = x;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode();
        }
    }

    class GetHashCodeEqualsOverride3 : GetHashCodeEqualsOverride2
    {
        private readonly int x;
        public GetHashCodeEqualsOverride3(int x) : base(x)
        {
            this.x = x;
        }
        public override int GetHashCode()
        {
            return x.GetHashCode() ^ base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }

    class Base
    { }

    class Derived : Base
    {
        public override int GetHashCode()
        {
            return base.GetHashCode(); //Noncompliant, calls object.GetHashCode()
        }
    }
}
