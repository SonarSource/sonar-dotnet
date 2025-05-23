namespace Nancy.Demo.Authentication.Forms
{
    using System;
    using System.Dynamic;

    using Nancy.Authentication.Forms;
    using Nancy.Extensions;

    public class MainModule : NancyModule
    {
        public MainModule()
        {
            Get("/", args => {
                return View["index"];
            });

            Get("/login", args =>
            {
                dynamic model = new ExpandoObject();
                model.Errored = this.Request.Query.error.HasValue;

                return View["login", model];
            });

            Post("/login", args => {
                var userGuid = UserDatabase.ValidateUser((string)this.Request.Form.Username, (string)this.Request.Form.Password);

                if (userGuid == null)
                {
                    return this.Context.GetRedirect("~/login?error=true&username=" + (string)this.Request.Form.Username);
                }

                DateTime? expiry = null;
                if (this.Request.Form.RememberMe.HasValue)
                {
                    expiry = DateTime.Now.AddDays(7);
                }

                return this.LoginAndRedirect(userGuid.Value, expiry);
            });

            Get("/logout", args => {
                return this.LogoutAndRedirect("~/");
            });
        }
    }
}