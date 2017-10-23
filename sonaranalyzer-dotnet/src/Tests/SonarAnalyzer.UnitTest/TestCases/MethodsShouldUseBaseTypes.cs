using System;
using System.Collections.Generic;


// Do not suggest ICollection<KVP<T1, T2>> instead of Dictionary<T1, T2>
namespace Test_25
{
    public class Foo
    {
        public void Foo(Dictionary<string, string> dictionary)
        {
            var x = dictionary.Count;
        }

        public void Bar(ICollection<KeyValuePair<string, string>> b)
        {
            var x = b.Count;
        }
    }
}