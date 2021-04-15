// version: FromCSharp8
namespace Tests.Diagnostics{
    public interface MyInterface1
    {
        public void Method1() { }
    }

    public class Class1
    {
        private interface MyInterface2 // Noncompliant
        {
            public void Method1() { }
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3842
    public class ReproIssue3842
    {
        public void SomeMethod()
        {
            ForSwitchArm x = null;
            var result = (x) switch
            {
                null => 1,
                // normally, when deconstructing in a switch, we don't actually know the type
                (object x1, object x2) => 2
            };
            var y = new ForIsPattern();
            if (y is (string a, string b)) { }
        }

        private sealed class ForSwitchArm
        {
            public void Deconstruct(out object a, out object b) { a = b = null; } // Noncompliant FP
        }

        private sealed class ForIsPattern
        {
            public void Deconstruct(out string a, out string b) { a = b = null; } // Noncompliant FP
        }
    }
}

// See: https://github.com/SonarSource/sonar-dotnet/issues/4102
namespace Repro1144
{
    using Microsoft.Extensions.DependencyInjection;

    public interface IMyService { }
    public interface ISomeExternalDependency {}

    public static class UIServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultMyService(this IServiceCollection services)
        {
            services.AddSingleton<IMyService, MyServiceSingleton>();
            services.AddScoped<IMyService, MyServiceScoped>();
            services.AddTransient<IMyService, MyServiceTransient>();

            return services;
        }

        private class MyServiceSingleton : IMyService
        {
            private readonly ISomeExternalDependency dependency;

            public MyServiceSingleton(ISomeExternalDependency dependency) => this.dependency = dependency; // Noncompliant FP
        }

        private class MyServiceScoped : IMyService
        {
            private readonly ISomeExternalDependency dependency;

            public MyServiceScoped(ISomeExternalDependency dependency) => this.dependency = dependency; // Noncompliant FP
        }

        private class MyServiceTransient : IMyService
        {
            private readonly ISomeExternalDependency dependency;

            public MyServiceTransient(ISomeExternalDependency dependency) => this.dependency = dependency; // Noncompliant FP
        }
    }
}
