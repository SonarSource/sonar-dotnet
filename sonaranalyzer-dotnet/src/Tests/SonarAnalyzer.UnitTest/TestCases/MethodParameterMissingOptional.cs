using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Tests.TestCases
{
    public class MethodParameterMissingOptional
    {
        public void MyMethod([DefaultParameterValue(5)] int j) //Noncompliant
//                            ^^^^^^^^^^^^^^^^^^^^^^^^
        {
            Console.WriteLine(j);
        }
        public void MyMethod2([DefaultParameterValue(5), Optional] int j)
        {
            Console.WriteLine(j);
        }
        public void MyMethod3([DefaultParameterValue(5)][Optional] int j)
        {
            Console.WriteLine(j);
        }
        public int this[[DefaultParameterValue(5)]int index] //Noncompliant {{Add the 'Optional' attribute to this parameter.}}
        {
            get { return 42; }
        }
    }
}
