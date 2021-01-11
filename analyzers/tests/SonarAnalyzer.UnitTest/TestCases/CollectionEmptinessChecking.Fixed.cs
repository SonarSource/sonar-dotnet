using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    public class Extenion_method_for_any
    {
        public bool Count_greater_then_zero(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        public bool Count_greater_then_or_equal_to_one(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        public bool Count_not_zero(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        public bool Zero_less_then_count(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        public bool One_less_then_or_equal_to_count(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        public bool Zero_not_count(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
    }
    public class Extenion_method_for_empty
    {
        public bool Count_equals_zero(IEnumerable<string> l)
        {
            return !l.Any(); // Fixed
        }
        public bool Count_less_then_one(IEnumerable<string> l)
        {
            return !l.Any(); // Fixed
        }
        public bool Count_less_then_or_equal_to_zero(IEnumerable<string> l)
        {
            return !l.Any(); // Fixed
        }
        public bool Zero_equals_count(IEnumerable<string> l)
        {
            return !l.Any(); // Fixed
        }
        public bool One_greater_then_count(IEnumerable<string> l)
        {
            return !l.Any(); // Fixed
        }
        public bool Zero_greater_then_or_equal_to_count(IEnumerable<string> l)
        {
            return !l.Any(); // Fixed
        }
    }
    public class Count_with_condition_specified
    {
        public bool Any_for_condition(List<int> numbers)
        {
            return numbers.Any(n => n % 2 == 0); // Fixed
        }
        public bool Empty_for_condition(List<int> numbers)
        {
            return !numbers.Any(n => n % 2 == 0); // Fixed
        }
    }
    public class Enumerable_count_for_any
    {
        public bool Count_greater_then_0(List<string> l)
        {
            return Enumerable.Any(l); // Fixed
        }
        public bool Zero_less_then_Count(List<string> l)
        {
            return Enumerable.Any(l); // Fixed
        }
    }
    public class Alternative_one_constants
    {
        public bool Count_greater_then_or_equal_binary_1(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        public bool Count_greater_then_or_equal_heximal_1(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
        }
        public bool UInt64_one_less_then_or_equal_to_count(IEnumerable<string> l)
        {
            return l.Any(); // Fixed
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
