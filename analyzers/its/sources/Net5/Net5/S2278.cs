using System.Security.Cryptography;

namespace Net5
{
    public class S2278
    {
        public void Foo()
        {
            using (DESCryptoServiceProvider des = new ())
            {
            }
        }
    }
}
