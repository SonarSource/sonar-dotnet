using System;

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
        private static string StaticField1;

        private string Field0_1;       // Noncompliant, never read nor written
        private string Field0_2 = "1"; // Noncompliant, written by initializer but not read
        private string Field0_3 = "1";
        private string Field1;
        private string Field2;
        private string Field3;
        private string Field4;
        private string Field5;
        private string Field6_1;
        private string Field6_2;
        private string Field6_3;
        private string Field7_1;
        private string Field7_2;
        private string Field7_3;
        private string Field8;
        private string Field9;
        private string Field10;
        private string Field11;
        private string[] Field12;
        private string Field13_1;
        private string Field13_2;
        private string Field14_1;
        private string Field14_2;
        private string Field15_1;
        private string Field15_2;

        public UsingCoalescingAssignmentOperator(string sPar) { }

        public string this[string sPar] => null;

        public void SomeMethod()
        {
            _ = Field0_3;
            _ = Field1 ??= "1";
            _ = Field2 ??= Field2;
            _ = Field3 ?? "1";
            _ = Field4 ?? (Field4 ??= "1");
            _ = Field5;
            _ = Field5 ??= Field5;
            _ = Field6_1 ??= Field6_2 ??= Field6_3;
            _ = Field7_1 ??= (Field7_2 ?? Field7_3);
            _ = "1" + (Field8 ??= "1");
            _ = OtherMethod(Field9 ??= "1");
            _ = this[Field10 ??= "1"];
            _ = this.Field11 ?? "1";
            _ = Field12[0] ??= "1";
            _ = Field13_1 ??= (Field13_2 += "1");
            _ = Field14_1 ??= (Field14_2 = "1");
            _ = Field15_1 ??= OtherMethod(Field15_2 = "1");
        }

        public static UsingCoalescingAssignmentOperator StaticMethod() => new UsingCoalescingAssignmentOperator(StaticField1 ??= "1");

        private string OtherMethod(string sPar) => null;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8342
public class Repro_8342
{
    [Private1] private protected void APrivateProtectedMethod() { }
    [Public1, Private2] public void APublicMethodWithMultipleAttributes1() { }
    [Public1][Private2] public void APublicMethodWithMultipleAttributes2() { }

    private class Private1Attribute : Attribute { }
    private class Private2Attribute : Attribute { }
    private class Private3Attribute : Attribute { }  // Noncompliant
    public class Public1Attribute : Attribute { }    // Compliant: public
}
