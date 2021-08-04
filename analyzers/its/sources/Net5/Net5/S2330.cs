namespace Net5
{
    public class S2330
    {
        private abstract class Fruit { }
        private class Apple : Fruit { }
        private class Orange : Fruit { }

        public void TestCases(bool isTrue)
        {
            Fruit[] fruits = isTrue
                ? new Apple[1]
                : new Orange[1];
        }
    }
}
