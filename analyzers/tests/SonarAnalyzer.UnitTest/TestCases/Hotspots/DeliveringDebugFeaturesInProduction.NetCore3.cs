using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;

namespace NetCore3
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Invoking as extension methods
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Compliant
            }
        }

        public void Configure2(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Invoking as static methods
            if (HostEnvironmentEnvExtensions.IsDevelopment(env))
            {
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Compliant
            }
        }

        public void Configure3(IApplicationBuilder app, IWebHostEnvironment env)
        {
            // Not in development
            if (!env.IsDevelopment())
            {
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // FN
            }
        }

        public void Configure4(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var isDevelopment = env.IsDevelopment();
            if (isDevelopment)
            {
                app.UseDeveloperExceptionPage();
            }
        }

        public void Configure5(IApplicationBuilder app, IWebHostEnvironment env)
        {
            while (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                break;
            }
        }

        public void Configure6(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseDeveloperExceptionPage(); // Noncompliant
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
        }

        public void Configure7(IApplicationBuilder app, IWebHostEnvironment env)
        {
            var x = env.IsDevelopment();
            app.UseDeveloperExceptionPage(); // FN
        }

        public void ConfigureAsArrow(IApplicationBuilder app, IWebHostEnvironment env) =>
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
    }

    public class StartupDevelopment
    {
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env) =>
            // See: https://docs.microsoft.com/en-us/aspnet/core/fundamentals/environments?view=aspnetcore-5.0#startup-class-conventions
            app.UseDeveloperExceptionPage(); // Compliant, it is inside StartupDevelopment class
    }
}
