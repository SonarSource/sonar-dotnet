using System;

namespace Tests.Diagnostics
{
    public class SwitchDefaultClauseEmpty
    {
        public enum Fruit
        {
            Apple,
            Orange,
            Banana
        }


        public SwitchDefaultClauseEmpty(Fruit fruit)
        {
            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                //Noncompliant@+1
                default: break;
//              ^^^^^^^^^^^^^^^
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                //Noncompliant@+1
                case Fruit.Orange:
                default:
                    break;
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                default:
                    // Single line comment before
                    break;
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                default:
                    break; // Single line comment after
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                default:
                    /* Multi lines comment before */
                    break;
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                default:
                    break; /* Multi lines comment after */
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                case Fruit.Orange:
                default:
                    /*commented*/
                    break;
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                default: // Single line comment after
                    break;
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                default:
                    {
                        // comment
                    }
                    break;
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
                default:
                    Console.WriteLine("other");
                    break;
            }
        }
    }
}
