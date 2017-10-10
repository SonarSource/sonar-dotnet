using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    class MyType
    {
        public static MyType operator ==(MyType x, MyType y) // Noncompliant {{Remove this overload of 'operator =='.}}
//                                    ^^
        {
        }
    }

    class MyTypeWithAdditionOverload
    {
        public static MyTypeWithAdditionOverload operator +(MyTypeWithAdditionOverload x, MyTypeWithAdditionOverload y)
        {
            return new MyTypeWithAdditionOverload();
        }

        public static bool operator ==(MyTypeWithAdditionOverload x, MyTypeWithAdditionOverload y) // Compliant
        {
            return false;
        }
    }

    class MyTypeWithSubstractionOverload
    {
        public static MyTypeWithSubstractionOverload operator -(MyTypeWithSubstractionOverload x, MyTypeWithSubstractionOverload y)
        {
            return new MyTypeWithSubstractionOverload();
        }

        public static bool operator ==(MyTypeWithSubstractionOverload x, MyTypeWithSubstractionOverload y) // Compliant
        {
            return false;
        }
    }

    struct ComparableTypeMyStruct
    {
        public static bool operator ==(MyStruct x, MyStruct y) // Compliant
        {
            return false;
        }
    }

    class GenericComparableType : IComparable<GenericComparableType>
    {
        public int CompareTo(GenericComparableType other)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(GenericComparableType x, GenericComparableType y) // Compliant
        {
            return false;
        }
    }

    class ComparableType : IComparable
    {
        public int CompareTo(object obj)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(ComparableType x, ComparableType y) // Compliant
        {
            return false;
        }
    }

    class EquatableType : IEquatable<EquatableType>
    {
        public bool Equals(ComparableType other)
        {
            throw new NotImplementedException();
        }

        public static bool operator ==(EquatableType x, EquatableType y) // Compliant
        {
            return false;
        }
    }
}
