using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyLibrary
{
    public class Foo
    {
        public virtual string GetFooMethod() => ""; // Noncompliant
    }

    public class Bar : Foo, IEnumerable<string>
    {
        private string name;

        public string GetName()
//                    ^^^^^^^ Noncompliant {{Consider making method 'GetName' a property.}}
        {
            return name;
        }

        public override string GetFooMethod() => ""; // Compliant - override method

        public IEnumerator<string> GetEnumerator() => null; // Compliant - comes from interface

        IEnumerator IEnumerable.GetEnumerator() => null; // Compliant - explicit interface implementation

        public object Get() => null;

        private string GetName2() => null;

        public string[] GetName3() => null;

        public object GetName4() => null; // Noncompliant

        public List<int> GetName5() => null; // Noncompliant

        public List<int> NameGet6() => null;

        public string GetName7(int param) => null;

        protected string GetName8() => null; // Noncompliant

        protected internal string GetName9() => null; // Noncompliant

        internal string GetName10() => null;

        public string GetProperty1 { get; set; }

        public string Property1 { get; set; }

        public string GetProperty2 { get { return ""; } }

        public string Property2 { get { return ""; } }
    }

    public interface IBase
    {
        string GetStuff(); // Noncompliant
    }

    public struct SomeStruct
    {
        public string GetStuff() { return ""; } // Noncompliant
    }

    public interface IFoo
    {
        // See https://github.com/SonarSource/sonar-dotnet/issues/1593
        // and https://msdn.microsoft.com/en-us/magazine/mt797654.aspx
        IMyEnumerator GetEnumerator();
    }

    public interface IMyEnumerator
    {
        int Current { get; }
        bool MoveNext();
    }

    // See https://github.com/SonarSource/sonar-dotnet/issues/2238
    #region GetAwaiter
    public class CustomAwaiter
    {
        public CustomAwaiter(int simulateDelayMilliseconds)
        {
        }
    }

    public class CustomAwaiter<T> : CustomAwaiter
    {
        public CustomAwaiter(int simulateDelayMilliseconds, T result)
            : base(simulateDelayMilliseconds)
        {
        }
    }

    public class CustomAwaitable
    {
        protected readonly int _simulateDelayMilliseconds;

        public CustomAwaitable(int simulateDelayMilliseconds)
        {
            _simulateDelayMilliseconds = simulateDelayMilliseconds;
        }

        public CustomAwaiter GetAwaiter() // Compliant, name is excluded
        {
            return new CustomAwaiter(_simulateDelayMilliseconds);
        }
    }

    public class CustomAwaitable<T> : CustomAwaitable
    {
        private readonly T _result;

        public CustomAwaitable(int simulateDelayMilliseconds, T result)
            : base(simulateDelayMilliseconds)
        {
            _result = result;
        }

        public new CustomAwaiter<T> GetAwaiter() // Compliant, name is excluded
        {
            return new CustomAwaiter<T>(_simulateDelayMilliseconds, _result);
        }
    }
    #endregion // GetAwaiter

    public class AsyncTests
    {
        public async void GetResultAsync() // Compliant - use async
        {
            await Task.FromResult(true).ConfigureAwait(false);
        }

        public Task GetSomething1() => Task.FromResult(true); // Compliant - return type is Task
        public Task<bool> GetSomething2() => Task.FromResult(true); // Compliant - return type is Task<T>
        public ValueTask<bool> GetSomething3() => default(ValueTask<bool>); // Compliant - return type is ValueTask<T>
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/3140
    public class Repro_3136
    {
        private static readonly Random rnd = new Random();

        [NoProperty]
        public int GetRandom() => rnd.Next(); // Compliant

        [NoProperty]
        [PropertyOrMethod]
        public string GetBothSupportedAndNotSupportedPropertyUsage() => ""; // Compliant

        [PropertyOrMethod]
        public int GetUsageIncludingProperty() => 42; // Noncompliant

        [AllTargets]
        public int GetUsageForAll() => 17; // Noncompliant

        [AllTargets]
        [PropertyOrMethod]
        public int GetUsageForAll2() => 17; // Noncompliant

        [NoUsageDefined]
        public int GetNoUsagedDefined() => 666; // Noncompliant

        [AttributeUsage(AttributeTargets.All)]
        private sealed class AllTargetsAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.Property | AttributeTargets.Method)]
        private sealed class PropertyOrMethodAttribute : Attribute { }

        [AttributeUsage(AttributeTargets.All ^ AttributeTargets.Property)]
        private sealed class NoPropertyAttribute : Attribute { }

        private sealed class NoUsageDefinedAttribute : Attribute { }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/8739
public class Repro_8739
{
    private object[] items;

    public IEnumerable<T> GetNodes<T>() =>
        Calculate<T>();

    public IEnumerable<T> GetNodesOriginal<T>() where T : Exception =>
        items.OfType<T>();

    private static IEnumerable<T> Calculate<T>() => null;

    public class GenericType<T>
    {
        public IEnumerable<T> GetNodes() =>  // Noncompliant, this can be a property
            Calculate<T>();
    }
}
