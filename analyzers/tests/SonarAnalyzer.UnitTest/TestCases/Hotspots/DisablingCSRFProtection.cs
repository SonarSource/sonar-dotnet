using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

namespace TestCases
{
    public class DisablingCSRFProtection
    {
        public static void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews(options => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute())); // Noncompliant
            services.AddControllersWithViews(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
            services.AddControllersWithViews(options => options.Filters.Add(new ValidateAntiForgeryTokenAttribute()));
            services.AddControllers(options => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute())); // Noncompliant
            services.AddMvc(options => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute())); // Noncompliant
            services.AddMvcCore(options => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute())); // Noncompliant

            Action<MvcOptions> setupAction = options => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute()); // Noncompliant
            services.AddControllersWithViews(setupAction);

            static void SetupAction(MvcOptions options) => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute()); // Noncompliant
            services.AddControllersWithViews(SetupAction);

            services.AddMvc(options => options.Filters.Add(GetAntiForgeryPolicy()));

            IgnoreAntiforgeryTokenAttribute _ = new(); // Noncompliant
        }

        private static IAntiforgeryPolicy GetAntiForgeryPolicy() =>
            new IgnoreAntiforgeryTokenAttribute(); // Noncompliant
    }

    [IgnoreAntiforgeryToken] // Noncompliant
    public class S4502Controller : Controller
    {
        private readonly string path = string.Empty;

        [IgnoreAntiforgeryToken] // Noncompliant
        public IActionResult ChangeEmail_Noncompliant(string model) => View(path);

        [HttpPost, IgnoreAntiforgeryTokenAttribute] // Noncompliant
        public IActionResult ChangeEmail_AttributesOnTheSameLine_Noncompliant(string model) => View(path);

        [HttpPost]
        [AutoValidateAntiforgeryToken]
        public IActionResult ChangeEmail_Compliant(string model) => View(path);

        [HttpPost, AutoValidateAntiforgeryToken]
        public IActionResult ChangeEmail_AttributesOnTheSameLine_Compliant(string model) => View(path);
    }
}
