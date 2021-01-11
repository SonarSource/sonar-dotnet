using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class Extenion_method_for_any
    {
        public bool Count_greater_then_zero(IEnumerable<string> l)
        {
            return l.Count() > 0; // Noncompliant {{Use '.Any()' to test whether this 'IEnumerable<string>' is empty or not.}}
            //       ^^^^^
        }
        public bool Count_greater_then_or_equal_to_one(IEnumerable<string> l)
        {
            return l.Count() >= 1; // Noncompliant
        }
        public bool Count_not_zero(IEnumerable<string> l)
        {
            return l.Count() != 0; // Noncompliant
        }
        public bool Zero_less_then_count(IEnumerable<string> l)
        {
            return 0 < l.Count(); // Noncompliant
        }
        public bool One_less_then_or_equal_to_count(IEnumerable<string> l)
        {
            return 1 <= l.Count(); // Noncompliant
        }
        public bool Zero_not_count(IEnumerable<string> l)
        {
            return 0 != l.Count(); // Noncompliant
        }
    }
    public class Extenion_method_for_empty
    {
        public bool Count_equals_zero(IEnumerable<string> l)
        {
            return l.Count() == 0; // Noncompliant
        }
        public bool Count_less_then_one(IEnumerable<string> l)
        {
            return l.Count() < 1; // Noncompliant
        }
        public bool Count_less_then_or_equal_to_zero(IEnumerable<string> l)
        {
            return l.Count() <= 0; // Noncompliant
        }
        public bool Zero_equals_count(IEnumerable<string> l)
        {
            return 0 == l.Count(); // Noncompliant
        }
        public bool One_greater_then_count(IEnumerable<string> l)
        {
            return 1 > l.Count(); // Noncompliant
        }
        public bool Zero_greater_then_or_equal_to_count(IEnumerable<string> l)
        {
            return 0 >= l.Count(); // Noncompliant
        }
    }
    public class Count_with_condition_specified
    {
        public bool Any_for_condition(List<int> numbers)
        {
            return numbers.Count(n => n % 2 == 0) > 0; // Noncompliant
        }
        public bool Empty_for_condition(List<int> numbers)
        {
            return numbers.Count(n => n % 2 == 0) == 0; // Noncompliant
        }
    }
    public class Enumerable_count_for_any
    {
        public bool Count_greater_then_0(List<string> l)
        {
            return Enumerable.Count(l) > 0; // Noncompliant
            //                ^^^^^
        }
        public bool Zero_less_then_Count(List<string> l)
        {
            return 0 < Enumerable.Count(l); // Noncompliant
        }
    }
    public class Alternative_one_constants
    {
        public bool Count_greater_then_or_equal_binary_1(IEnumerable<string> l)
        {
            return l.Count() >= 0b1; // Noncompliant
        }
        public bool Count_greater_then_or_equal_heximal_1(IEnumerable<string> l)
        {
            return l.Count() >= 0x1; // Noncompliant
        }
        public bool UInt64_one_less_then_or_equal_to_count(IEnumerable<string> l)
        {
            return 1UL <= l.Count(); // Noncompliant // Error [CS0034]
        }
    }
    public class Local_count_method
    {
        public int Count(IEnumerable<string> l) => 3;
        public bool Count_equals_zero(IEnumerable<string> l)
        {
            return Count(l) == 0; // Compliant
        }
    }
    public class Count_for_multiple
    {
        public bool Count_more_then_one(IEnumerable<string> l)
        {
            return l.Count() > 1; // Compliant
        }
    }
}
