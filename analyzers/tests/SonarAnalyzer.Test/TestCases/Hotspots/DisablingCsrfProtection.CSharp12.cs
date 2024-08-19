using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;

public class DisablingCSRFProtection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers((MvcOptions options = null) => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute())); // Noncompliant
    }
}
