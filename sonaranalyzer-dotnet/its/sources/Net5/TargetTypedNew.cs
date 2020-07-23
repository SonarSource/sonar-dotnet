using System;
using System.Collections.Generic;
using System.Text;

namespace Net5
{
    public class TargetTypedNew
    {
        private Dictionary<string, string> dict = new();
        private List<int> list;

        public void Method()
        {
            list = new();
            StringBuilder sb = new();
            StringBuilder sb2 = new(null);
            StringBuilder sb3 = new(length: 4, capacity: 3, startIndex: 1, value: "fooBar");
            Console.WriteLine(sb.ToString() + sb2.ToString() + sb3.ToString());
        }
    }
}
