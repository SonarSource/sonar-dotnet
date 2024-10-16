using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.DependencyInjection;
using IATA = Microsoft.AspNetCore.Mvc.IgnoreAntiforgeryTokenAttribute;

public class DisablingCSRFProtection
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllersWithViews(options => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute())); // Noncompliant
        services.AddControllersWithViews(options => options.Filters.Add(new IATA())); // FN - for performance reasons type alias is not supported
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

internal class Inheritance 
{
    [DerivedAttribute] // FN - for performance reasons inheritance is not supported
    public void B() { }
}

public class DerivedAttribute : IgnoreAntiforgeryTokenAttribute { }

internal class TestCases
{
    public void Bar(IEnumerable<int> collection)
    {
        [IgnoreAntiforgeryToken] int Get() => 1; // Noncompliant

        _ = collection.Select([IgnoreAntiforgeryToken] (x) => x + 1); // Noncompliant

        Func<int, int> nonCompliantDelegate = [IgnoreAntiforgeryToken] (x) => x + 1; // Noncompliant

        Action a = [IgnoreAntiforgeryToken] () => { }; // Noncompliant

        Action x = true
                       ? ([IgnoreAntiforgeryToken] () => { }) // Noncompliant
                       : [IgnoreAntiforgeryToken] () => { }; // Noncompliant

        Call([IgnoreAntiforgeryToken] (x) => { }); // Noncompliant
    }

    private void Call(Action<int> action) => action(1);
}

internal class SomeTest2
{
    [GenericIgnoreAntiforgeryToken<int>]    // FN: for performance reasons inheritance is not supported
    public void A() { }

    [IgnoreAntiforgeryTokenAttribute]       // Noncompliant
    public void B() { }
}

public class GenericIgnoreAntiforgeryToken<T> : IgnoreAntiforgeryTokenAttribute { }

public class DisablingCSRFProtection2
{
    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers((MvcOptions options = null) => options.Filters.Add(new IgnoreAntiforgeryTokenAttribute())); // Noncompliant
    }
}

partial class Partial
{
    public IEnumerable<int> collection;

    [IgnoreAntiforgeryToken] // Noncompliant
    partial int Get => 1; // Error@-1 [CS0592]

    partial int Get2
    {
        get
        {
            return collection.Select([IgnoreAntiforgeryToken] (x) => x + 1).FirstOrDefault(); // Noncompliant
        }
    }
}

partial class Partial
{
    partial int Get { get; }
    partial int Get2 { get; }
}
