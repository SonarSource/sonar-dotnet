using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Microsoft.Azure.WebJobs
{
    public class FunctionNameAttribute : Attribute { public FunctionNameAttribute(string name) { } }  // FIXME: Fake, remove before merging
}

public class AnotherAttribute : Attribute
{
    public AnotherAttribute(string name) { }
}

public static class Functions
{
    private const int ConstantInt = 42;

    delegate void VoidDelegate();

    public static void NotAnAzureFunction()     // Compliant
    {
        DoSomething();
    }

    [Another("Something")]
    public static void WithAnotherAttribute()   // Compliant
    {
        DoSomething();
    }

    [FunctionName("FIXME")]
    public static void NoTryCatch_Body(int arg)         // Noncompliant {{Wrap Azure Function body in try/catch block.}}
    //                 ^^^^^^^^^^^^^^^
    {
        DoSomething();
    }

    [FunctionName("FIXME")]
    public static void NoTryCatch_Arrow() =>            // Noncompliant
        DoSomething();

    [FunctionName("FIXME")]
    public static async Task<string> NoTryCatchAsync()  // Noncompliant
    {
        return DoSomething();
    }

    [FunctionName("FIXME")]
    public static void Empty()
    {
        // Compliant. Nothing to see here
    }

    [FunctionName("FIXME")]
    public static async Task<int> Harmless()
    {
        Action notInvokedParenthesizedLambda = () => { DoSomething(); };    // Compliant, not invoked
        Action<int> notInvokedSimpleLambda = x => { DoSomething(); };       // Compliant, not invoked
        VoidDelegate notInvokedAnonymousMethod = delegate                   // Compliant, not invoked
        {
            DoSomething();
        };

        var ret = 42 + ConstantInt;
        ret -= int.MaxValue;
        return ret;

        string LocalNotInvoked() =>                     // Compliant, not invoked
            DoSomething();

        static string StaticLocalNotInvoked() =>        // Compliant, not invoked
            DoSomething();
    }

    [FunctionName("FIXME")]
    public static void PropertyAccess(ICollection<int> items)   // This is considered compliant for simplicity. Properties should not throw.
    {
        if (items.Count == 0)
        {
            return;
        }
    }

    [FunctionName("FIXME")]
    public static void TryFinally()   // Noncompliant
    {
        try
        {
            DoSomething();
        }
        finally
        {
        }
    }

    private static string DoSomething() => null;    // Compliant, btw
}

public static class AttributeVariants
{
    [FunctionNameAttribute("ClassName")]
    public static void ClassName()          // Noncompliant
    {
        DoSomething();
    }

    [Microsoft.Azure.WebJobs.FunctionNameAttribute("NamespaceName")]
    public static void NamespaceName()      // Noncompliant
    {
        DoSomething();
    }

    [global::Microsoft.Azure.WebJobs.FunctionNameAttribute("GlobalName")]
    public static void GlobalName()         // Noncompliant
    {
        DoSomething();
    }

    private static void DoSomething() { }
}

public static class CatchScenarios
{
    [FunctionName("FIXME")]
    public static void OuterCatch()
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (Exception ex)
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchFullName()
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (System.Exception ex)
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchGlobalName()
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (global::System.Exception ex)
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchNoVariable()
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (Exception)
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchNoType()
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchSpecific()   // Noncompliant
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (NullReferenceException ex)
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchSpecificAndAll()
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (NullReferenceException ex)
        {
        }
        catch (Exception ex)
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchSpecificAndAll_When()   // FIXME FN Non-compliant
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (NullReferenceException ex)
        {
        }
        catch (Exception ex) when (ex is ArgumentNullException)
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchWhen()   // FIXME FN Non-compliant
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (Exception ex) when (ex is ArgumentNullException)
        {
        }
    }

    [FunctionName("FIXME")]
    public static void OuterCatchWhenAlwaysTrue()   // FIXME FN Non-compliant FP, we don't support constant condition tracking
    {
        try
        {
            var i = 42;
            DoSomething();
        }
        catch (Exception ex) when (true)
        {
        }
    }

    private static void DoSomething() { }
}

public static class NestedTry
{
    [FunctionName("FIXME")]
    public static void NestedCatch(int arg)  // Compliant
    {
        if (arg == 42)
        {
            try
            {
                DoSomething();
            }
            catch
            {
            }
        }
    }

    [FunctionName("FIXME")]
    public static void NestedCatch_Multi(int arg)  // Compliant
    {
        if (arg == 42)
        {
            try
            {
                DoSomething();
            }
            catch
            {
            }
        }
        else
        {
            try
            {
                DoSomething();
            }
            catch
            {
            }
        }
    }

    [FunctionName("FIXME")]
    public static void NestedCatch_Deep(int arg)  // Compliant
    {
        if (arg == 42)
        {
            if (arg != 0)
            {
                while (true)
                {
                    try
                    {
                        DoSomething();
                    }
                    catch
                    {
                    }
                    break;
                }
            }
        }
    }

    [FunctionName("FIXME")]
    public static void InvocationOutside_BeforeTry(int arg)   // Noncompliant
    {
        if (arg == 42)
        {
            DoSomething();
            try
            {
                DoSomething();
            }
            finally
            {
            }
        }
    }

    [FunctionName("FIXME")]
    public static void InvocationOutside_BeforeIf(int arg)   // Noncompliant
    {
        DoSomething();
        if (arg == 42)
        {
            try
            {
                DoSomething();
            }
            finally
            {
            }
        }
    }

    [FunctionName("FIXME")]
    public static void InvocationOutside_AfterTry(int arg)   // Noncompliant
    {
        if (arg == 42)
        {
            try
            {
                DoSomething();
            }
            finally
            {
            }
            DoSomething();
        }
    }

    [FunctionName("FIXME")]
    public static void InvocationOutside_AfterIf(int arg)   // Noncompliant
    {
        if (arg == 42)
        {
            try
            {
                DoSomething();
            }
            finally
            {
            }
        }
        DoSomething();
    }

    [FunctionName("FIXME")]
    public static void InvocationOutside_NestedDeep(int arg)  // Noncompliant
    {
        if (arg == 42)
        {
            if (arg != 0)
            {
                while (true)
                {
                    try
                    {
                        DoSomething();
                    }
                    catch
                    {
                    }
                    DoSomething();
                    break;
                }
            }
        }
    }

    private static void DoSomething() { }
}

public class NonStatic
{
    [FunctionName("FIXME")]
    public void InstanceMethod()         // Noncompliant, should not be decorated with the attribute anyway
    {
        DoSomething();
    }

    [FunctionName("FIXME")]
    public static void StaticMethod()   // Noncompliant
    {
        DoSomething();
    }

    private static void DoSomething() { }
}
