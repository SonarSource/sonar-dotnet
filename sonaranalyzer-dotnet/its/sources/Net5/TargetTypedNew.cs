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
            Console.WriteLine(sb);
        }
    }
}
