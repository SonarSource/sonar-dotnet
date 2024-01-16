using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace SonarAnalyzer.Test.TestCasesForRuleFailure
{
    public class SpecialCases
    {
        public void ParamsMethod(int i, params int[] j) { }
        public int MethodExpressionBody(int i, params int[] j) => 42;
        public int PropertyExpressionBody => 42;

        public void ArgListMethod(__arglist)
        {
            ArgListMethod(__arglist(""));
        }
        public void DynamicMethod(dynamic i)
        {
            dynamic local = new object();
        }

        public static implicit operator string(SpecialCases c) { return ""; }
        public static SpecialCases operator+ (SpecialCases a, SpecialCases b) { return null; }
        public int this[int index] => index;

        [DllImport("User32.dll", CharSet = CharSet.Unicode)]
        public static extern int MessageBox(IntPtr h, string m, string c, int type);

        static int Main()
        {
            return MessageBox((IntPtr)0, "My message", "My Message Box", 0);
        }
    }

    public static class MyExtensions
    {
        public static void Ext(this string s)
        {
            string ss = null;
            ss.Ext();
            Ext(null);
            dynamic d = ss;
            d.Ext();
            Ext(d);
        }

        public static Dictionary<string, string> GetDict()
        {
            return new Dictionary<string, string>
            {
                ["a"] = "b"
            };

        }
    }
}
