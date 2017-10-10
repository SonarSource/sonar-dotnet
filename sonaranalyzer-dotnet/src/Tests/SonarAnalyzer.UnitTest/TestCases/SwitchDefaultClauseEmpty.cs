namespace Tests.Diagnostics
{
    public class SwitchDefaultClauseEmpty
    {
        enum Fruit
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
                case Fruit.Orange: // Noncompliant
                default:
                    /*commented*/
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