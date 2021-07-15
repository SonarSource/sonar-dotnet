using System;

namespace Net5
{
    public class S3928
    {
        ArgumentException exception = new ("a", "foo");
        public int Foo9
        {
            init
            {
                throw new ArgumentNullException("value");
                throw new ArgumentNullException("foo");
                throw new ArgumentNullException("Foo9");
            }
        }
    }
}
