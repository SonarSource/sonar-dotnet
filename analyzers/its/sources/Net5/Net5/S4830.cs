namespace Net5
{
    public class S4830
    {
        void Foo()
        {
            using (var ms = new System.IO.MemoryStream())
            using (System.Net.Security.SslStream ssl = new (ms, true, (sender, chain, certificate, SslPolicyErrors) => true))
            {
            }
        }
    }
}
