using Nancy.Cookies;

namespace Net5
{
    public class S2092
    {
        NancyCookie cookie = new ("name", "value", secure: false, httpOnly: true);
    }
}
