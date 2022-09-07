using System;
using System.Collections.Generic;

namespace Tests.Diagnostics.ComparableInterfaceImplementation
{
    public class Compliant : IComparable // Compliant
    {
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Compare(this, obj as Compliant);
        }

        private static int Compare(Compliant left, Compliant right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Compliant;
            return other != null && this.Name != other.Name;
        }

        public static bool operator ==(Compliant left, Compliant right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Compliant left, Compliant right)
        {
            return !(left == right);
        }

        public static bool operator <(Compliant left, Compliant right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(Compliant left, Compliant right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator >=(Compliant left, Compliant right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(Compliant left, Compliant right)
        {
            return Compare(left, right) <= 0;
        }
    }

    public class DerivedCompliant : Compliant, IComparable // Compliant
    {
    }

    public class MissingEquals : IComparable // Noncompliant {{When implementing IComparable, you should also override Equals.}}
//               ^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Compare(this, obj as MissingEquals);
        }

        private static int Compare(MissingEquals left, MissingEquals right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public static bool operator ==(MissingEquals left, MissingEquals right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissingEquals left, MissingEquals right)
        {
            return !(left == right);
        }

        public static bool operator <(MissingEquals left, MissingEquals right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(MissingEquals left, MissingEquals right)
        {
            return Compare(left, right) > 0;
        }
        public static bool operator >=(MissingEquals left, MissingEquals right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(MissingEquals left, MissingEquals right)
        {
            return Compare(left, right) <= 0;
        }

    }

    public class DifferentEquals : IComparable // Noncompliant {{When implementing IComparable, you should also override Equals.}}
//               ^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Compare(this, obj as DifferentEquals);
        }

        private static int Compare(DifferentEquals left, DifferentEquals right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public bool Equals(object obj, string someParam)
        {
            var other = obj as DifferentEquals;
            return other != null && this.Name != other.Name;
        }

        public bool Equals(object obj)
        {
            return true;
        }

        public bool Equals()
        {
            return true;
        }

        public bool Equals { get; set; } // Error [CS0102]

        public bool Equals => true; // Error [CS0102]

        public bool Equals() => true; // Error [CS0102,CS0111]

        public static bool operator ==(DifferentEquals left, DifferentEquals right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DifferentEquals left, DifferentEquals right)
        {
            return !(left == right);
        }

        public static bool operator <(DifferentEquals left, DifferentEquals right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(DifferentEquals left, DifferentEquals right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator >=(DifferentEquals left, DifferentEquals right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(DifferentEquals left, DifferentEquals right)
        {
            return Compare(left, right) <= 0;
        }
    }

    public class MissingGreaterThan : IComparable // Noncompliant {{When implementing IComparable, you should also override < and >.}}
//               ^^^^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Compare(this, obj as MissingGreaterThan);
        }

        private static int Compare(MissingGreaterThan left, MissingGreaterThan right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MissingGreaterThan;
            return other != null && this.Name != other.Name;
        }

        public static bool operator ==(MissingGreaterThan left, MissingGreaterThan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissingGreaterThan left, MissingGreaterThan right)
        {
            return !(left == right);
        }

        public static bool operator >=(MissingGreaterThan left, MissingGreaterThan right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(MissingGreaterThan left, MissingGreaterThan right)
        {
            return Compare(left, right) <= 0;
        }
    }

    public class MissingNotEqual : IComparable // Noncompliant {{When implementing IComparable, you should also override == and !=.}}
//               ^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Compare(this, obj as MissingNotEqual);
        }

        private static int Compare(MissingNotEqual left, MissingNotEqual right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MissingNotEqual;
            return other != null && this.Name != other.Name;
        }

        public static bool operator <(MissingNotEqual left, MissingNotEqual right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(MissingNotEqual left, MissingNotEqual right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator >=(MissingNotEqual left, MissingNotEqual right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(MissingNotEqual left, MissingNotEqual right)
        {
            return Compare(left, right) <= 0;
        }
    }

    public class MissingGreaterThanOrEqualTo : IComparable // Noncompliant {{When implementing IComparable, you should also override >=.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Compare(this, obj as MissingGreaterThanOrEqualTo);
        }

        private static int Compare(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MissingNotEqual;
            return other != null && this.Name != other.Name;
        }

        public static bool operator <(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator ==(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            return !(left == right);
        }

        public static bool operator <=(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right) // Error [CS0216]
        {
            return Compare(left, right) <= 0;
        }
    }

    public class MissingLessThanOrEqualTo : IComparable // Noncompliant {{When implementing IComparable, you should also override <=.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return Compare(this, obj as MissingLessThanOrEqualTo);
        }

        private static int Compare(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MissingNotEqual;
            return other != null && this.Name != other.Name;
        }

        public static bool operator <(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator ==(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            return !(left == right);
        }

        public static bool operator >=(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right) // Error [CS0216]
        {
            return Compare(left, right) >= 0;
        }
    }

    public struct Struct : IComparable // Noncompliant {{When implementing IComparable, you should also override Equals, ==, !=, <, <=, >, and >=.}}
    {
        public int CompareTo(object obj)
        {
            return 0;
        }
    }
}

namespace Tests.Diagnostics.ComparableGenericInterfaceImplementation
{
    public class Compliant : IComparable<Compliant> // Compliant
    {
        public string Name { get; set; }

        public int CompareTo(Compliant obj)
        {
            return Compare(this, obj);
        }

        private static int Compare(Compliant left, Compliant right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Compliant;
            return other != null && this.Name != other.Name;
        }

        public static bool operator ==(Compliant left, Compliant right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Compliant left, Compliant right)
        {
            return !(left == right);
        }

        public static bool operator <(Compliant left, Compliant right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(Compliant left, Compliant right)
        {
            return Compare(left, right) > 0;
        }
        public static bool operator >=(Compliant left, Compliant right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(Compliant left, Compliant right)
        {
            return Compare(left, right) <= 0;
        }

    }

    public class MissingEquals : IComparable<MissingEquals> // Noncompliant {{When implementing IComparable<T>, you should also override Equals.}}
//               ^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(MissingEquals obj)
        {
            return Compare(this, obj);
        }

        private static int Compare(MissingEquals left, MissingEquals right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public static bool operator ==(MissingEquals left, MissingEquals right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissingEquals left, MissingEquals right)
        {
            return !(left == right);
        }

        public static bool operator <(MissingEquals left, MissingEquals right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(MissingEquals left, MissingEquals right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator >=(MissingEquals left, MissingEquals right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(MissingEquals left, MissingEquals right)
        {
            return Compare(left, right) <= 0;
        }
    }

    public class DifferentEquals : IComparable<DifferentEquals> // Noncompliant {{When implementing IComparable<T>, you should also override Equals.}}
//               ^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(DifferentEquals obj)
        {
            return Compare(this, obj);
        }

        private static int Compare(DifferentEquals left, DifferentEquals right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public bool Equals(object obj, string someParam)
        {
            var other = obj as DifferentEquals;
            return other != null && this.Name != other.Name;
        }

        public bool Equals(object obj)
        {
            return true;
        }

        public static bool operator ==(DifferentEquals left, DifferentEquals right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(DifferentEquals left, DifferentEquals right)
        {
            return !(left == right);
        }

        public static bool operator <(DifferentEquals left, DifferentEquals right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(DifferentEquals left, DifferentEquals right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator >=(DifferentEquals left, DifferentEquals right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(DifferentEquals left, DifferentEquals right)
        {
            return Compare(left, right) <= 0;
        }
    }

    public class MissingGreaterThan : IComparable<MissingGreaterThan> // Noncompliant {{When implementing IComparable<T>, you should also override < and >.}}
//               ^^^^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(MissingGreaterThan obj)
        {
            return Compare(this, obj);
        }

        private static int Compare(MissingGreaterThan left, MissingGreaterThan right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MissingGreaterThan;
            return other != null && this.Name != other.Name;
        }

        public static bool operator ==(MissingGreaterThan left, MissingGreaterThan right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissingGreaterThan left, MissingGreaterThan right)
        {
            return !(left == right);
        }

        public static bool operator >=(MissingGreaterThan left, MissingGreaterThan right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(MissingGreaterThan left, MissingGreaterThan right)
        {
            return Compare(left, right) <= 0;
        }
    }

    public class MissingNotEqual : IComparable<MissingNotEqual> // Noncompliant {{When implementing IComparable<T>, you should also override == and !=.}}
//               ^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(MissingNotEqual obj)
        {
            return Compare(this, obj);
        }

        private static int Compare(MissingNotEqual left, MissingNotEqual right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MissingNotEqual;
            return other != null && this.Name != other.Name;
        }

        public static bool operator <(MissingNotEqual left, MissingNotEqual right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(MissingNotEqual left, MissingNotEqual right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator >=(MissingNotEqual left, MissingNotEqual right)
        {
            return Compare(left, right) >= 0;
        }

        public static bool operator <=(MissingNotEqual left, MissingNotEqual right)
        {
            return Compare(left, right) <= 0;
        }
    }

    public class MissingGreaterThanOrEqualTo : IComparable<MissingGreaterThanOrEqualTo> // Noncompliant {{When implementing IComparable<T>, you should also override >=.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(MissingGreaterThanOrEqualTo obj)
        {
            return Compare(this, obj);
        }

        private static int Compare(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MissingNotEqual;
            return other != null && this.Name != other.Name;
        }

        public static bool operator <(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator ==(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right)
        {
            return !(left == right);
        }

        public static bool operator <=(MissingGreaterThanOrEqualTo left, MissingGreaterThanOrEqualTo right) // Error [CS0216]
        {
            return Compare(left, right) <= 0;
        }
    }

    public class MissingLessThanOrEqualTo : IComparable<MissingLessThanOrEqualTo> // Noncompliant {{When implementing IComparable<T>, you should also override <=.}}
//               ^^^^^^^^^^^^^^^^^^^^^^^^
    {
        public string Name { get; set; }

        public int CompareTo(MissingLessThanOrEqualTo obj)
        {
            return Compare(this, obj);
        }

        private static int Compare(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }

        public override bool Equals(object obj)
        {
            var other = obj as MissingNotEqual;
            return other != null && this.Name != other.Name;
        }

        public static bool operator <(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            return Compare(left, right) < 0;
        }

        public static bool operator >(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            return Compare(left, right) > 0;
        }

        public static bool operator ==(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right)
        {
            return !(left == right);
        }

        public static bool operator >=(MissingLessThanOrEqualTo left, MissingLessThanOrEqualTo right) // Error [CS0216]
        {
            return Compare(left, right) >= 0;
        }
    }
}

namespace Tests.Diagnostics.BothInterfacesImplementation
{
    public class NonCompliant : IComparable, IComparable<NonCompliant> // Noncompliant {{When implementing IComparable or IComparable<T>, you should also override Equals, ==, !=, <, <=, >, and >=.}}
    {
        public string Name { get; set; }

        public int CompareTo(object obj)
        {
            return 0;
        }

        public int CompareTo(NonCompliant obj)
        {
            return Compare(this, obj);
        }

        private static int Compare(NonCompliant left, NonCompliant right)
        {
            if (left == null && right == null)
            {
                return 0;
            }

            if (left == null)
            {
                return -1;
            }

            if (right == null)
            {
                return 1;
            }

            return left.Name.CompareTo(right.Name);
        }
    }
}
