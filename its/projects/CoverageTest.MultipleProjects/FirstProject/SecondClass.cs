using System;

namespace FirstProject
{
    public class SecondClass
    {
        public void CallFirstClass(FirstClass bar)
        {
            Console.WriteLine(bar.CoveredGet);
            Console.WriteLine(bar.CoveredProperty);
        }
    }
}
