using System.Collections.Generic;

namespace Tests.Diagnostics
{
    public class ShortCircuitNullPointerDereference
    {
        public object O { get; set; }

        public Test(int a, int b)
        {
            if (O == null && new ShortCircuitNullPointerDereference().O == null)
            {
                
            }

            List<int> l = null;
            if ( someOtherCondition && null == l && l.Count()>5)  // Noncompliant
            {
                Console.WriteLine("Message");
            }

            TreeNode t = new TreeNode();

            while(someOtherCondition && (t != /*Comment*/ null || t.Parent /*Comment*/ != null)) // Noncompliant
            {
                Console.WriteLine("Going up");
                t = t.Parent;
            }
        }
    }
}
