using System;
using System.Collections.Generic;

namespace Net5
{
    public class S3267
    {
        public List<string> SuggestWhere(ICollection<string> collection, Predicate<string> condition)
        {
            var result = new List<string>();

            foreach (var element in collection) // Noncompliant
            {
                if (condition(element))
                {
                    result.Add(element);
                }
            }

            return result;
        }

        public List<int> SuggestSelectAndWhere(ICollection<string> collection)
        {
            var result = new List<int>();

            foreach (var element in collection) // Noncompliant
            {
                var someValue = element.Length;
                if (someValue > 0)
                {
                    result.Add(someValue);
                }
            }

            return result;
        }
    }
}
