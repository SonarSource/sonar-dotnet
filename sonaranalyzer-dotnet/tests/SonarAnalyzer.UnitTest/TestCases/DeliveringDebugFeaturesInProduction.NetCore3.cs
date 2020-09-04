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

            // Invoking as static methods
            if (HostEnvironmentEnvExtensions.IsDevelopment(env))
            {
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Compliant
            }

            // Not in development
            if (!env.IsDevelopment())
            {
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
            }

            // Custom conditions are deliberately ignored
            var isDevelopment = env.IsDevelopment();
            if (isDevelopment)
            {
                app.UseDeveloperExceptionPage(); // Noncompliant, False Positive
            }

            while (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Noncompliant FP, we inspect only IF statements
                break;
            }

            // These are called unconditionally
            app.UseDeveloperExceptionPage(); // Noncompliant
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
        }

        public void ConfigureAsArrow(IApplicationBuilder app, IWebHostEnvironment env) =>
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
    }
}
