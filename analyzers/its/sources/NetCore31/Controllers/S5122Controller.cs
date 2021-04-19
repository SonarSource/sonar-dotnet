using Microsoft.AspNetCore.Mvc;

namespace NetCore31.Controllers
{
    public class S5122Controller : Controller
    {
        public void PermissiveHeader()
        {
            Response.Headers.Add("Access-Control-Allow-Origin", "*");
        }
    }
}
