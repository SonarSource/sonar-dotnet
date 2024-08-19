using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Program
{
    private object field = null;
    private static object staticField = null;

    public void NotCompliantCases(object o, Exception e)
    {
        o.ToString();   // Noncompliant {{Refactor this method to add validation of parameter 'o' before using it.}}
//      ^
        Bar(o);         // Compliant, we care about dereference only

        throw e;        // FN - the SE engine uses the throw statement as branch rather than an operation
    }

    public void Bar(object o) { }

    protected void NotCompliantCases_Protected(object o)
    {
        o.ToString(); // Noncompliant
    }

    private void CompliantCases_Private(object o)
    {
        o.ToString(); // Compliant - not accessible from other assemblies
    }

    protected internal void NotCompliantCases_ProtectedInternal(object o)
    {
        o.ToString(); // Noncompliant
    }

    internal void CompliantCases_Internal(object o)
    {
        o.ToString(); // Compliant - not accessible from other assemblies
    }

    public void CompliantCases(bool b, object o1, object o2, object o3, object o4, Exception e)
    {
        if (o1 != null)
        {
            o1.ToString();  // Compliant, we did the check
        }

        o2 = o2 ?? new object();
        o2.ToString();      // Compliant

        if (o3 == null)
        {
            throw new Exception();
        }

        o3.ToString();                  // Compliant, we did the check

        if (e != null)
        {
            throw e;                    // Compliant
        }

        o4?.ToString();                 // Compliant, conditional operator

        b.ToString();                   // Compliant, bool cannot be null

        object v = null;
        v.ToString();                   // Compliant, we don't care about local variables

        field.ToString();               // Compliant

        Program.staticField.ToString(); // Compliant
    }

    public void MoreCompliantCases(string s1, string s2)
    {
        if (string.IsNullOrEmpty(s1))
        {
            s1.ToString(); // Null check was performed, so this belongs to S2259
        }
        else
        {
            s1.ToString();
        }

        if (string.IsNullOrWhiteSpace(s2))
        {
            s2.ToString(); // Null check was performed, so this belongs to S2259
        }
        else
        {
            s2.ToString();
        }
    }

    public void ForEachLoop(object[] array)
    {
        foreach (object o in array) // Noncompliant
        {
        }
    }

    public async void AsyncTest(Task task1, Task task2, Task task3, Task task4)
    {
        if (task1 != null)
        {
            await task1;
        }

        await task2;                                // Noncompliant

        if (task3 != null)
        {
            await task3.ConfigureAwait(false);
        }

        await task4.ConfigureAwait(false);          // Noncompliant
    }

    public Program(int i) { }

    public Program(string s) : this(s.Length) { }   // Noncompliant {{Refactor this constructor to avoid using members of parameter 's' because it could be null.}}

    public void NonCompliant1(object o)
    {
        var c = o?.ToString()?.IsNormalized();
        if (c == null)
        {
            o.GetType().GetMethods();               // Null check was performed, so this belongs to S2259
        }
    }

    public void Compliant1(object o)
    {
        var c = o?.ToString()?.IsNormalized();
        if (c != null)
        {
            o.GetType().GetMethods();
        }
    }

    public void WithDelegate(object asParameter, object asVariable)
    {
        SomeAction(asParameter.ToString);       // Noncompliant
        Func<string> f = asVariable.ToString;   // Noncompliant

        void SomeAction(Func<string> a)
        { }
    }
}

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class ValidatedNotNullAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class FooAttribute : Attribute { }

[AttributeUsage(AttributeTargets.Parameter)]
public sealed class BarAttribute : Attribute { }

public static class GuardExtensionClass
{
    public static void GuardExtension([ValidatedNotNull] this string value) { }
    public static void GuardExtensionMoreAttributes([Foo][ValidatedNotNull][Bar] this string value) { }
    public static void GuardExtensionMoreArguments([ValidatedNotNull] this string value, string foo, string bar) { }
}

public class GuardedTests
{
    public void Guarded(string s1, string s2, string s3, string s4, string s5, string s6, string s7)
    {
        Guard1(s1);
        s1.ToUpper();

        Guard2(s2, "s2");
        s2.ToUpper();

        Guard3("s3", s3);
        s3.ToUpper();

        Guard4(s4);
        s4.ToUpper();

        s5.GuardExtension();
        s5.ToUpper();

        s6.GuardExtensionMoreAttributes();
        s6.ToUpper();

        s7.GuardExtensionMoreArguments(null, null);
        s7.ToUpper();
    }

    public void Guard1<T>([ValidatedNotNull] T value) where T : class { }

    public void Guard2<T>([ValidatedNotNull] T value, string name) where T : class { }

    public void Guard3<T>(string name, [ValidatedNotNull] T value) where T : class { }

    public static void Guard4<T>([ValidatedNotNull] T value) where T : class { }
}

public class ReproIssue2476
{
    public void Foo(params string[] infixes)
    {
        if (infixes == null || infixes.Length == 0)
        {
            infixes = new string[1];
        }
        else
        {
            RefMethod(ref infixes, infixes.Length); // Noncompliant - FP
        }
    }

    private void RefMethod(ref string[] array, int num) { }

    public void Method(ref string s, int x) { }
    public void Method1(string infixes)
    {
        if (infixes != null)
        {
            Method(ref infixes, infixes.Length);    // Noncompliant - FP
            var x = infixes.Length;
        }

    }

    public void Method2(string infixes)
    {
        if (infixes == null)
        {
            Method(ref infixes, infixes.Length);    // Noncompliant
            var x = infixes.Length;
        }
    }

    public void Method3(string infixes)
    {
        Method(ref infixes, infixes.Length);        // Noncompliant
        var x = infixes.Length;
    }

    public void Method4(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentException("inputString cannot be null or empty", contentType);
        }

        if (contentType.Equals("*"))
        {
            contentType = "*/*";
        }

        var parts = contentType.Split('/', ';');
    }

    public void Method5(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentException("inputString cannot be null or empty", contentType);
        }

        if (contentType.Equals("*"))
        {
            contentType = "";
        }

        var parts = contentType.Split('/', ';'); // Covered by S2259
    }

    public void Method6(string contentType)
    {
        if (string.IsNullOrEmpty(contentType))
        {
            throw new ArgumentException("inputString cannot be null or empty", contentType);
        }

        if (contentType.Equals("*"))
        {
            contentType = null;
        }

        var parts = contentType.Split('/', ';'); // Covered by S2259
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/2591
public class ReproIssue2591
{
    public string FooWithStringTrim(string name)
    {
        if (name == null)
        {
            name = Guid.NewGuid().ToString("N");
        }

        return name.Trim();
    }

    public string FooWithStringJoin(string name)
    {
        if (name == null)
        {
            name = Guid.NewGuid().ToString("N");
        }

        return string.Join("_", name.Split(System.IO.Path.GetInvalidFileNameChars()));
    }

    public string FooWithObject(object name)
    {
        if (name == null)
        {
            name = Guid.NewGuid().ToString("N");
        }

        return name.ToString();
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/2670
public class ReproIssue2670
{
    public static void BooleanEqualityComparisonIsNullOrEmpty(string argument, bool b)
    {
        if (string.IsNullOrEmpty(argument) == true)
        {
            return;
        }
        if (b)
        {
            int index = argument.LastIndexOf('c');
        }
    }

    public static void BooleanEqualityComparisonWithIsNullOrWhiteSpace(string argument, bool b)
    {
        if (string.IsNullOrWhiteSpace(argument) == true)
        {
            return;
        }
        if (b)
        {
            int index = argument.LastIndexOf('c');
        }
    }

    public static void NoComparison(string argument, bool b)
    {
        if (string.IsNullOrEmpty(argument))
        {
            return;
        }
        if (b)
        {
            int index = argument.LastIndexOf('c');
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/2775
public class ReproWithIs
{
    public override bool Equals(object obj)
    {
        var equals = (obj is string) && (obj.GetHashCode() == GetHashCode());
        if (equals)
        {
            // do stuff
        }
        return equals;
    }
}

public class ReproWithIsNullOrEmpty
{
    public static string SanitiseIMDbId(string imdbId)
    {
        if (string.IsNullOrEmpty(imdbId))
            return string.Empty;

        if (imdbId.StartsWith(" "))
            imdbId = string.Concat("tt", imdbId);

        if (imdbId.Length != 9)
            imdbId = string.Empty;

        return imdbId;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/3400
public class Repro_3400
{
    public void ReassignedFromMethod(StringBuilder parameter)
    {
        parameter = Create();
        parameter.Capacity = 1;
    }

    public void ReassignedFromConstructor(StringBuilder parameter)
    {
        parameter = new StringBuilder();
        parameter.Capacity = 1;
    }

    public void ReassignedFromMethodOut(out StringBuilder parameter)
    {
        parameter = Create();
        parameter.Capacity = 1;
    }

    public void ReassignedFromConstructorOut(out StringBuilder parameter)
    {
        parameter = new StringBuilder();
        parameter.Capacity = 1;
    }

    private StringBuilder Create() => null;
}

public class ClassAccessibility
{
    private object field = null;

    public void PublicWithoutArgs()
    {
        field.ToString(); // Compliant - not a method argument
    }

    public void PublicWithArgs(object o)
    {
        o.ToString(); // Noncompliant
    }

    protected void ProtectedWithArgs(object o)
    {
        o.ToString(); // Noncompliant
    }

    protected internal void ProtectedInternalWithArgs(object o)
    {
        o.ToString(); // Noncompliant
    }

    internal void InternalWithArgs(object o)
    {
        o.ToString(); // Compliant - method is not accessible from other assemblies
    }

    private void PrivateWithArgs(object o)
    {
        o.ToString();
    }

    void ImplicitlyPrivateWithArgs(object o)
    {
        o.ToString(); // Compliant
    }
}

public struct StructAccessibility
{
    private object field;

    public void PublicWithoutArgs()
    {
        field.ToString(); // Compliant - not a method argument
    }

    public void PublicWithArgs(object o)
    {
        o.ToString(); // Noncompliant
    }

    internal void InternalWithArgs(object o)
    {
        o.ToString(); // Compliant - method is not accessible from other assemblies
    }

    void ImplicitlyInternalWithArgs(object o)
    {
        o.ToString();
    }

    private void PrivateWithArgs(object o)
    {
        o.ToString();
    }

    public void LambdasAndAnonymousDelegates()
    {
        MethodAcceptsFunction(obj => obj.ToString());
        MethodAcceptsFunction((obj) => obj.ToString());
        MethodAcceptsFunction(delegate (object obj) { obj.ToString(); });
    }

    private void MethodAcceptsFunction(Action<object> action) { }
}

public class PropertyAccessibility
{
    public object PublicProperty
    {
        get => null;
        set => _ = value.ToString();                    // Noncompliant
    }

    protected object ProtectedProperty
    {
        get => null;
        set => _ = value.ToString();                    // Noncompliant
    }

    protected internal object ProtectedInternalProperty
    {
        get => null;
        set => _ = value.ToString();                    // Noncompliant
    }

    internal object InternalProperty
    {
        get => null;
        set => _ = value.ToString();
    }

    private object PrivateProperty
    {
        get => null;
        set => _ = value.ToString();
    }

    object ImplicitlyPrivateProperty
    {
        get => null;
        set => _ = value.ToString();
    }

    public object ProtectedSetter
    {
        get => null;
        protected set => _ = value.ToString();          // Noncompliant
    }

    public object ProtectedInternalSetter
    {
        get => null;
        protected internal set => _ = value.ToString(); // Noncompliant
    }

    public object InternalSetter
    {
        get => null;
        internal set => _ = value.ToString();           // Compliant - setter is not accessible from other assemblies
    }

    public object PrivateSetter
    {
        get => null;
        private set => _ = value.ToString();
    }
}

public class ClassWithIndexer
{
    public string this[object index]
    {
        get { return index.ToString(); }                // Noncompliant
        set { _ = value.ToString(); }                   // Noncompliant
    }
}

public class ClassWithEvent
{
    public event EventHandler CustomEvent;

    public void Method(ClassWithEvent c)
    {
        c.CustomEvent += (sender, args)                 // Noncompliant
            => Console.WriteLine();
    }
}

internal class InternalClass
{
    public void PublicWithArgs(object o)
    {
        o.ToString();                                   // Compliant - method is not accessible from other assemblies
    }
}

class ImplicitlyInternalClass
{
    public void PublicWithArgs(object o)
    {
        o.ToString();
    }
}

public class NestedClasses
{
    public class PublicNestedClass
    {
        public void Method(object o)
        {
            o.ToString();                               // Noncompliant
        }

        private class DeeperNestedPrivateClass
        {
            public void Method(object o)
            {
                o.ToString();                           // Compliant - method is not accessible from other assemblies
            }
        }
    }

    protected class ProtectedNestedClass
    {
        public void Method(object o)
        {
            o.ToString();                               // Noncompliant
        }
    }

    private class PrivateNestedClass
    {
        public void Method(object o)
        {
            o.ToString();                               // Compliant - method is not accessible from other assemblies
        }

        public class DeeperNestedPublicClass
        {
            public void Method(object o)
            {
                o.ToString();                           // Compliant - method is not accessible from other assemblies
            }
        }
    }
}

public class Conversion
{
    public void DownCast(object o)
    {
        ((string)o).ToString();                             // Noncompliant
    }

    public void UpCast(string s)
    {
        ((object)s).ToString();                             // Noncompliant
    }

    public void CastWithMemberAccess(object o1, object o2, object o3, object o4)
    {
        _ = ((CustomClass)o1).Property;                     // Noncompliant
        ((CustomClass)o2).CustomEvent +=                    // Noncompliant
                (sender, args) => { };
        _ = ((CustomClass)o3).field;                        // Noncompliant
        Func<string> method = ((CustomClass)o4).ToString;   // Noncompliant
    }

    public void CastWithRedundantParentheses(object o)
    {
        (((string)o)).ToString();                           // Noncompliant
    }

    public void MultipleCasts(object o)
    {
        ((string)((object)((string)o))).ToString();         // Noncompliant
    }

    public void AsOperatorDownCast(object o)
    {
        (o as string).ToString();                           // Noncompliant
    }

    public void AsOperatorUpCast(string s)
    {
        (s as object).ToString();                           // Noncompliant
    }

    public void ForEachLoop(object[] arr, IEnumerable<object> enumerable)
    {
        foreach (object o in arr)                           // Noncompliant - the array is first cast to an IEnumerable, then the GetEnumerator method is invoked on it
        {
        }

        foreach (object o in enumerable)                    // Noncompliant
        {
        }
    }

    public void ForEachLoopWithCast(object[] arr)
    {
        foreach (object o in (IEnumerable<object>)arr)      // Noncompliant
        {
        }
    }

    public void ForEachLoopWithParentheses(object[] arr)
    {
        foreach (object o in ((arr)))                       // Noncompliant
        {
        }
    }

    public void ForEachLoopWithCastAndParentheses(object[] arr)
    {
        foreach (object o in ((object[])(object)((arr))))   // Noncompliant
        {
        }
    }

    private class CustomClass
    {
        public int field;
        public int Property { get; set; }
        public event EventHandler CustomEvent;
    }
}

public class Constructor : Base
{
    public Constructor(string s)
    {
        _ = s.Length;           // Noncompliant {{Refactor this method to add validation of parameter 's' before using it.}}
    }

    public Constructor(object o) :
        this(o.ToString())      // Noncompliant {{Refactor this constructor to avoid using members of parameter 'o' because it could be null.}}
    {
    }

    public Constructor(Exception e)
    {
        new Base(e.ToString()); // Noncompliant {{Refactor this method to add validation of parameter 'e' before using it.}}
    }

    public Constructor(object[] o) :
        base(o.ToString())      // Noncompliant {{Refactor this constructor to avoid using members of parameter 'o' because it could be null.}}
    {
    }

    public Constructor(List<object> l) :
        base(l.Count > 0 ? "not empty" : "empty")      // Noncompliant {{Refactor this constructor to avoid using members of parameter 'l' because it could be null.}}
    {
    }
}

public class Base
{
    public Base() { }
    public Base(string s) { }
}

public class DereferencedMultipleTimesOnTheSameExecutionPath
{
    public void UsedOnSameExecutionPath(string s)
    {
        s.ToLower();            // Noncompliant
        s.ToLower();            // Compliant - s was dereferenced in the previous line, so it's not null here
    }

    public void UsedOnMultipleLevels(string s)
    {
        s.Replace(              // Compliant - SubString was already called on s when we call Replace, so s is not null
            "a",
            s.Substring(1));    // Noncompliant
    }

    public void UsedTwiceOnSameLevel(string s)
    {
        _ = s.Substring(        // Compliant
            s.IndexOf("a"),     // Noncompliant
            s.IndexOf("b"));    // Compliant - IndexOf("a") was called before this method call, so s is not null here
    }
}

public class ParameterAssignment
{
    public void ParameterIsAssignedNewValue(object o)
    {
        o = Unknown();
        o.ToString();           // Compliant - we're no longer validating the original value of the parameter
    }

    public void PassedAsRefToMethod(object o)
    {
        RefMethod(ref o);
        o.ToString();           // Noncompliant - FP: o was passed by reference to a method, so it may have been assigned a new value
    }

    public void ConditionalAssignment(object o)
    {
        o = o == null ? Unknown() : o;
        o.ToString();           // Noncompliant - FP: Flow Capture is not handled
    }

    public void TupleDeconstruction(object o)
    {
        (o, _) = (Unknown(), new object());
        o.ToString();           // Noncompliant - FP
    }

    private object Unknown() => null;
    private void RefMethod(ref object objectRef) { }
}

public class ParameterCaptures
{
    public string InsideLambda_CaptureAfterCheck(string s)
    {
        if (s == null)
        {
            return null;
        }

        Func<string> someFunc = () => s.ToString();

        return s.ToString(); // Compliant
    }

    public string InsideLambda_CaptureBeforeDereference(string s)
    {
        Func<string> someFunc = () => s.ToString();

        return s.ToString(); // FN: s is not checked here
    }

    public void CapturedInLinq_WithCheckBefore(string s, List<string> list)
    {
        if (s == null)
        {
            return;
        }
        s.ToString(); // Compliant

        list.Where(x => x == s);
    }

    public void CapturedInLinq_WithNoCheck(string s, List<string> list)
    {
        s.ToString(); // FN

        list.Where(x => x == s);
    }
}

public class Keywords
{
    public void Method(object @event)
    {
        @event.ToString(); // Noncompliant {{Refactor this method to add validation of parameter '@event' before using it.}}
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/9500
public class Repro_9500
{
    void Method(List<int> list)
    {
        _ = list.Where(x => true);  // FN
    }
}
