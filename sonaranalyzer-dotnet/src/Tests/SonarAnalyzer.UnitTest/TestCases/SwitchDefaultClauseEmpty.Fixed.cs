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
            }

            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
                    break;
            }
            switch (fruit)
            {
                case Fruit.Apple:
                    Console.WriteLine("apple");
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