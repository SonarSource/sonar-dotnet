namespace Net5
{
    public class S2219
    {
        record Fruit { }
        sealed record Apple : Fruit { }

        public void Foo()
        {
            Apple apple = new();
            Fruit f = apple;
            var b = f is Apple;
            b = f as Apple is null; 
        }
    }
}
