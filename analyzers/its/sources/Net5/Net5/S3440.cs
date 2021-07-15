namespace Net5
{
    public class S3440
    {
        public void Foo()
        {
            int y = 6;
            y = y switch
            {
                5 => 5,
                6 => 6
            };
        }

        private int? f;
        public int Property
        {
            get
            {
                if (f != null)
                {
                    f = null;
                }
                return 1;
            }
            init
            {
                if (f != null)
                {
                    f = null;
                }
            }
        }
    }
}
