using System;
using System.Collections.Generic;
using System.Text;

namespace Net5
{
    public class TargetTypedNew
    {
        Dictionary<string, string> dict = new();
        List<int> list;

        public void Method()
        {
            list = new();
            StringBuilder sb = new();
            Console.WriteLine(sb);
        }
    }
}
