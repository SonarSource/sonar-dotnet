using System;
using System.Collections.Generic;
using System.Linq;

namespace Tests.Diagnostics
{
    internal class TreeNode
    {
        public TreeNode Parent { get; set; }
    }

    public class ShortCircuitNullPointerDereference
    {
        public object O { get; set; }

        public void Test(int a, int b)
        {
            if (O == null && new ShortCircuitNullPointerDereference().O == null)
            {

            }

            List<int> l = new List<int>();
            bool someOtherCondition = a== b;

            if ( someOtherCondition && null == l && l.Count()>5)  // Noncompliant {{Either reverse the equality operator in the 'l' null test, or reverse the logical operator that follows it.}}
//                                     ^^^^^^^^^
            {
                Console.WriteLine("Message");
            }

            var t = new TreeNode();

            while(someOtherCondition && (t != /*Comment*/ null || t.Parent /*Comment*/ != null)) // Noncompliant
            {
                Console.WriteLine("Going up");
                t = t.Parent;
            }
        }
    }
}
