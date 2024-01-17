using System;
using System.Collections.Generic;
using System.ComponentModel;
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

    class DerivedWithExpressionBody : Base
    {
        public override int GetHashCode() => base.GetHashCode(); //Noncompliant, calls object.GetHashCode()
        public override bool Equals(Object obj) => base.Equals(obj); // Noncompliant
    }


    /**
     * If the method has been annotated with an attribute, we should not raise, because the only way to annotate the
     * base behavior with an attribute is by overriding it.
     */

    class WithBrowsableAttribute
    {
        [Browsable(false)]
        public override int GetHashCode()
        {
            return base.GetHashCode(); // Compliant, it's decorating the base behavior
        }

        [Browsable(true)]
        public override bool Equals(Object obj)
        {
            return base.Equals(obj); // FN - the attribute isn't changing the default behavior
        }
    }

    class WithEditorBrowsableFalseAttribute
    {
        [EditorBrowsable(EditorBrowsableState.Always)]
        public override int GetHashCode()
        {
            return base.GetHashCode(); // FN - the attribute isn't changing the default behavior
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(Object obj)
        {
            return base.Equals(obj); // Compliant, it's decorating the base behavior
        }
    }

    class WithBothAttributes
    {
        [Browsable(false)]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode() => base.GetHashCode(); // Compliant
    }

    class WithCustomAttribute
    {
        [MyAttribute]
        public override int GetHashCode() => base.GetHashCode(); // Compliant, decorating the default behavior

        [MyAttribute]
        public override bool Equals(Object obj)
        {
            return base.Equals(obj); // Compliant, it's decorating the base behavior
        }
    }

    public class MyAttribute : Attribute { }

    // Testing different corner cases
    class EqualsEmptyReturnBase
    {
        protected bool Equals(object first, object second) { return true; }
    }

    class EqualsEmptyReturn : EqualsEmptyReturnBase
    {
        public override bool Equals(Object obj)
        {
            Method();

            Equals(obj);

            if (base.Equals(obj, obj))
            {
            }

            if (base.Equals(obj, obj) == false)
            {
            }

            if (base.Equals(obj)) // Noncompliant
            {
                return; // Error [CS0126]
            }

            return; // Error [CS0126]
        }

        private void Method() { }
    }

    public class GetHashCodeInsideExpressions
    {
        private readonly IDictionary<string, dynamic> dictionary = new Dictionary<string, dynamic>();

        public override int GetHashCode()
        {
            return dictionary != null ? dictionary.GetHashCode() : 0; // Compliant
        }
    }
}
