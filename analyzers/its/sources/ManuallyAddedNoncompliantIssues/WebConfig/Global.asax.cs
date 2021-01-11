using System.Web.Routing; // Noncompliant (S1451)

namespace Framework48
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start() => RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
}
