namespace Tests.Diagnostics
{
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

// https://github.com/SonarSource/sonar-dotnet/issues/4102
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

// https://github.com/SonarSource/sonar-dotnet/issues/6653
namespace Repro6653
{
    public class UsingCoalescingAssignmentOperator
    {
        private static string StaticField1; // Compliant, read and written by ??= in SomeStaticMethod

        private string Field0_1;            // Noncompliant, never read nor written
        private string Field0_2 = "1";      // Noncompliant, written by initializer but not read
        private string Field0_3 = "1";      // Compliant, read by Property0 and written by initializer
        private string Field1;              // Compliant, read and written by ??=
        private string Field2;              // Compliant, read twice and written by ??=
        private string Field3;              // Compliant, only read by ??
        private string Field4;              // Compliant, read by ?? and read and written by ??=
        private string Field5;              // Compliant, read in Property5_1 and read and written by ??= in Property5_2
        private string Field6_1;            // Compliant, read and written by 1st ??=
        private string Field6_2;            // Compliant, read by both ??= and written by 2nd ??=
        private string Field6_3;            // Compliant, read and written by 2nd ??=
        private string Field7_1;            // Compliant, read and written by ??=
        private string Field7_2;            // Compliant, read and written by ??
        private string Field7_3;            // Compliant, read and written by ??
        private string Field8;              // Compliant, read and written by ??=
        private string Field9;              // Compliant, read and written by ??=
        private string Field10;             // Compliant, read and written by ??=
        private string Field11;             // Compliant, read and written by ??=
        private string[] Field12;           // Compliant, item read and written by ??=
        private string Field13_1;           // Compliant, read and written by ??=
        private string Field13_2;           // Compliant, read and written by +=
        private string Field14_1;           // Compliant, read and written by ??=
        private string Field14_2;           // Compliant, written by = and read by ??=
        private string Field15_1;           // Compliant, read and written by ??=
        private string Field15_2;           // Compliant, written by = and read by SomeMethod

        public string Property0 => Field0_3;
        public string Property1 => Field1 ??= "1";
        public string Property2 => Field2 ??= Field2;
        public string Property3 => Field3 ?? "1";
        public string Property4 => Field4 ?? (Field4 ??= "1");
        public string Property5_1 => Field5;
        public string Property5_2 => Field5 ??= Field5;
        public string Property6 => Field6_1 ??= Field6_2 ??= Field6_3;
        public string Property7 => Field7_1 ??= (Field7_2 ?? Field7_3);
        public string Property8 => "1" + (Field8 ??= "1");
        public string Property9 => SomeMethod(Field9 ??= "1");
        public string Property10 => this[Field10 ??= "1"];
        public string Property11 => this.Field11 ?? "1";
        public string Property12 => Field12[0] ??= "1";
        public string Property13 => Field13_1 ??= (Field13_2 += "1");
        public string Property14 => Field14_1 ??= (Field14_2 = "1");
        public string Property15 => Field15_1 ??= SomeMethod(Field15_2 = "1");

        public UsingCoalescingAssignmentOperator(string sPar) { }

        public string this[string sPar] => sPar + "1";

        public static UsingCoalescingAssignmentOperator StaticProperty1() => new UsingCoalescingAssignmentOperator(StaticField1 ??= "1");

        private string SomeMethod(string sPar) => sPar + "1";
    }
}

