using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Example.Core
{
    public class Alex
    {
        public IMoney FirstMoney { get; set; }
        public IMoney SecondMoney { get; set; }

        public IMoney ThirdMoney { get; set; }
        public IMoney FourthMoney { get; set; }

        bool coucou()
        {
            return false;
        }

        void DisplayEmptyIf()
        {
            if (coucou())
            {
                // empty
            }
            Console.WriteLine("end");
        }

        void DisplayEmptyElse()
        {
            if (coucou())
            {
                Console.WriteLine("coucou");
            } else
            {
                // empty
            }

            Console.WriteLine("end");
        }

        void DisplayEmptyTry()
        {
            try
            {
                // empty try
            } finally
            {
                Console.WriteLine("end");    
            }
            
        }

        public void displayFirstMoney()
        {
            string toto = null;
            toto.ToString();

            if (toto=="")
            {
                Console.WriteLine("eee");
            } else
            {
                // rien
            }

            if (toto==null && toto.ToString()=="")
            {
                
            }



            try
            {
                Int32 ii = 1232;


            } catch(Exception e)
            {
                
            } finally
            {
                
            }

            Console.WriteLine(FirstMoney);
        }

        public void displaySecondMoney()
        {
            Console.WriteLine(SecondMoney);
        }

        private void weirdDisplayFirstMoney()
        {
            Console.WriteLine(FirstMoney);
        }

        public void displayHello()
        {
            Console.WriteLine("hello"+SecondMoney);
        }

        public IMoney GetMoney()
        {
            return new Money(123, "EUR");
        }

        public void displayHello2()
        {
            Console.WriteLine("hello2"+FourthMoney);
        }

        public void displayHello3()
        {
            Console.WriteLine("hello3"+ThirdMoney);
        }

        public void displayHello4()
        {
            Console.WriteLine("hello4"+SecondMoney);
        }

        public void displayHello5()
        {
            Console.WriteLine("hello5"+FirstMoney);
        }
    }
}
