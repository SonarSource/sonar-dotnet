using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// This is a comment!

namespace CSLib.bar
{
    /// <summary>
    /// Documented public API
    /// </summary>
    public class MyClazz
    {
        public int MyProperty
        {
            get
            {
                return 42;
            }
        }
    }

    class Class1
    {
        public void Main()
        {
            MyClazz myClazz = new MyClazz();
            Console.WriteLine("Hello, world! " + myClazz.MyProperty);
            Console.ReadLine();
        }
    }
}
