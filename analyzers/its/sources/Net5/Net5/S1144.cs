namespace Net5
{
    public class S1144
    {
        private S1144(int arg)
        {
            var x = arg;
        }

        private S1144(string arg)
        {
            var x = arg;
        }

        public static S1144 Create() =>
            new (42);
    }
}
