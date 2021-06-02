using JWT.Builder;

namespace Net5
{
    public class S5659
    {
        public void Foo()
        {
            JwtBuilder decoded1 = new ();
            decoded1.WithSecret("").Decode("");
        }
    }
}
