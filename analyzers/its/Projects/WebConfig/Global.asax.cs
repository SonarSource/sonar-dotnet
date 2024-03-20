using System.Web.Routing;

namespace Framework48
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start() => RouteConfig.RegisterRoutes(RouteTable.Routes);
    }
}
