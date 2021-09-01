using System;
using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class S3267
    {
        public List<string> SuggestWhere(ICollection<string> collection, Predicate<string> condition)
        {
            var result = new List<string>();

            foreach (var element in collection) // Noncompliant {{Loops should be simplified with "LINQ" expressions}}
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

            foreach (var element in collection)
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
