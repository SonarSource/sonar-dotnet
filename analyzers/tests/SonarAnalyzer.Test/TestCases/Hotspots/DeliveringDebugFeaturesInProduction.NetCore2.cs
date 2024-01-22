using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace Tests.Diagnostics
{
    public class Startup
    {
        // for coverage
        private IHostingEnvironment env;

        private IApplicationBuilder foo;
        public IApplicationBuilder Foo
        {
            get
            {
                if (env.IsDevelopment())
                {
                    foo.UseDeveloperExceptionPage();        // Compliant
                }
                return foo;
            }
            set
            {
                var x = value.UseDeveloperExceptionPage();  // Noncompliant
                foo = value;
            }
        }

        event EventHandler SomeEvent
        {
            add { foo.UseDeveloperExceptionPage(); }        // Noncompliant
            remove
            {
                if (env.IsDevelopment())
                {
                    foo.UseDeveloperExceptionPage();        // Compliant
                }
            }
        }

        public Startup()
        {
            foo.UseDeveloperExceptionPage();                // Noncompliant
        }

        public void Lambda()
        {
            Action action1 = () =>
            {
                foo.UseDeveloperExceptionPage();            // Noncompliant
            };
            action1();

            Action action2 = () =>
            {
                if (env.IsDevelopment())
                {
                    foo.UseDeveloperExceptionPage();        // Compliant
                }
            };
            action2();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Invoking as extension methods
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage(); // Compliant
                app.UseDatabaseErrorPage(); // Compliant
            }
        }

        public void Configure2(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Invoking as static methods
            if (HostingEnvironmentExtensions.IsDevelopment(env))
            {
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Compliant
                DatabaseErrorPageExtensions.UseDatabaseErrorPage(app); // Compliant
            }
        }

        public void Configure3(IApplicationBuilder app, IHostingEnvironment env)
        {
            // Not in development
            if (!env.IsDevelopment())
            {
                DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // FN
                DatabaseErrorPageExtensions.UseDatabaseErrorPage(app); // FN
            }
        }

        public void Configure4(IApplicationBuilder app, IHostingEnvironment env)
        {
            var isDevelopment = env.IsDevelopment();
            if (isDevelopment)
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
        }

        public void Configure5(IApplicationBuilder app, IHostingEnvironment env)
        {
            while (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
                break;
            }
        }

        public void Configure6(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseDeveloperExceptionPage(); // Noncompliant
            app.UseDatabaseErrorPage(); // Noncompliant
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
            DatabaseErrorPageExtensions.UseDatabaseErrorPage(app); // Noncompliant
        }

        public void Configure7(IApplicationBuilder app, IHostingEnvironment env)
        {
            var x = env.IsDevelopment();
            app.UseDeveloperExceptionPage(); // FN
            app.UseDatabaseErrorPage(); // FN
        }

        public void ConfigureAsArrow(IApplicationBuilder app, IHostingEnvironment env) =>
            DeveloperExceptionPageExtensions.UseDeveloperExceptionPage(app); // Noncompliant
    }
}
