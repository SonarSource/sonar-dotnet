using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Tests.Diagnostics
{
    public class Startup
    {
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Invoking as extension methods
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Compliant
                app.UseDatabaseErrorPage(); // Compliant
            }

            // Invoking as static methods
            if (HostingEnvironmentExtensions.IsDevelopment(env))
            {
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Compliant
                DatabaseErrorPageExtensions.UseDatabaseErrorPage(app); // Compliant
            }

            // Not in development
            if (!env.IsDevelopment())
            {
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
                DatabaseErrorPageExtensions.UseDatabaseErrorPage(app); // Noncompliant
            }

            // Custom conditions are deliberately ignored
            var isDevelopment = env.IsDevelopment();
            if (isDevelopment)
            {
                app.UseDeveloperExceptionPage(); // Noncompliant, False Positive
                app.UseDatabaseErrorPage(); // Noncompliant, False Positive
            }

            // These are called unconditionally
            app.UseDeveloperExceptionPage(); // Noncompliant
            app.UseDatabaseErrorPage(); // Noncompliant
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
            DatabaseErrorPageExtensions.UseDatabaseErrorPage(app); // Noncompliant
        }
    }
}
