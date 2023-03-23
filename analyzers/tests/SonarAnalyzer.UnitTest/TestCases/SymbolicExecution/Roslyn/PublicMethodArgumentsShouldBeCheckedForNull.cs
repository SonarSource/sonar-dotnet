using System;
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
        o.ToString(); // Noncompliant - FP (method visibility)
    }

    protected internal void NotCompliantCases_ProtectedInternal(object o)
    {
        o.ToString(); // Noncompliant
    }

    internal void CompliantCases_Internal(object o)
    {
        o.ToString(); // Noncompliant - FP (method visibility)
    }

    public void CompliantCases(bool b, object o1, object o2, object o3, object o4, Exception e)
    {
        if (o1 != null)
        {
            o1.ToString();  // Compliant, we did the check
        }

        o2 = o2 ?? new object();
        o2.ToString();      // Noncompliant - FP

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
            RefMethod(ref infixes, infixes.Length); // Noncompliant - FP: infixes is not null at this point
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

        return name.Trim(); // Noncompliant - FP
    }

    public string FooWithStringJoin(string name)
    {
        if (name == null)
        {
            name = Guid.NewGuid().ToString("N");
        }

        return string.Join("_", name.Split(System.IO.Path.GetInvalidFileNameChars())); // Noncompliant - FP
    }

    public string FooWithObject(object name)
    {
        if (name == null)
        {
            name = Guid.NewGuid().ToString("N");
        }

        return name.ToString(); // Noncompliant - FP
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

        if (imdbId.Length != 9) // Noncompliant - FP
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
        parameter.Capacity = 1; // Noncompliant - FP
    }

    public void ReassignedFromConstructor(StringBuilder parameter)
    {
        parameter = new StringBuilder();
        parameter.Capacity = 1;
    }

    public void ReassignedFromMethodOut(out StringBuilder parameter)
    {
        parameter = Create();
        parameter.Capacity = 1; // Noncompliant - FP
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
        o.ToString(); // FIXME Non-compliant
    }

    protected void ProtectedWithArgs(object o)
    {
        o.ToString(); // FIXME Non-compliant
    }

    protected internal void ProtectedInternalWithArgs(object o)
    {
        o.ToString(); // FIXME Non-compliant
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
        o.ToString(); // FIXME Non-compliant
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
}

class PropertyAccessibility
{
    public object PublicProperty
    {
        get => null;
        set => _ = value.ToString();                    // FIXME Non-compliant
    }

    protected object ProtectedProperty
    {
        get => null;
        set => _ = value.ToString();                    // FIXME Non-compliant
    }

    protected internal object ProtectedInternalProperty
    {
        get => null;
        set => _ = value.ToString();                    // FIXME Non-compliant
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
        protected set => _ = value.ToString();          // FIXME Non-compliant
    }

    public object ProtectedInternalSetter
    {
        get => null;
        protected internal set => _ = value.ToString(); // FIXME Non-compliant
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
        get { return index.ToString(); }                // FIXME Non-compliant
        set { _ = value.ToString(); }                   // FIXME Non-compliant
    }
}

public class ClassWithEvent
{
    public event EventHandler CustomEvent;

    public void Method(ClassWithEvent c)
    {
        c.CustomEvent += (sender, args)                 // FIXME Non-compliant
            => Console.WriteLine(); 
    }
}

public class NestedClasses
{
    protected class ProtectedNestedClass
    {
        public void Method(object o)
        {
            o.ToString();                               // FIXME Non-compliant
        }
    }

    private class PrivateNestedClass
    {
        public void Method(object o)                    
        {
            o.ToString();                               // Compliant - method is not accessible from other assemblies
        }
    }
}
