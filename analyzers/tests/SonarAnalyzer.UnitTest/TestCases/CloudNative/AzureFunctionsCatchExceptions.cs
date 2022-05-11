using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

namespace Microsoft.Azure.WebJobs
{
    public class FunctionNameAttribute { }  // FIXME: Fake, remove before merging
}

public static class Functions
{
    private const int ConstantInt = 42;

    [FunctionName("FIXME")]
    public static void NoTryCatch_Body(int arg)         // Noncompliant {{FIXME}}
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
        DoSomething();
    }

    [FunctionName("FIXME")]
    public static void Empty()
    {
        // Compliant. Nothing to see here
    }

    [FunctionName("FIXME")]
    public static Task<int> Harmless()
    {
        Action notInvoked = () => { DoSomething(); };  // Compliant, not invoked
        var ret = 42 + ConstantInt;
        ret -= int.MaxValue;
        return ret;

        int LocalNotInvoked() =>                        // Compliant, not invoked
            DoSomething();

        int StaticLocalNotInvoked() =>                  // Compliant, not invoked
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

    [FunctionName("FIXME")
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

    private void DoSomething() { }
}

public static class AttributeVariants
{
    [FunctionNameAttribute("ClassName")]
    public static void ClassName()
    {
        DoSomething();
    }

    [Microsoft.Azure.WebJobs.FunctionNameAttribute("NamespaceName")]
    public static void NamespaceName()
    {
        DoSomething();
    }

    [global::Microsoft.Azure.WebJobs.FunctionNameAttribute("GlobalName")]
    public static void GlobalName()
    {
        DoSomething();
    }
}

public static class CatchScenarios
{
    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
    public static void OuterCatchSpecificAndAll_When()   // Noncompliant
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

    [FunctionName("FIXME")
    public static void OuterCatchWhen()   // Noncompliant
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

    private void DoSomething() { }
}

public static class NestedTry
{
    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    [FunctionName("FIXME")
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

    private void DoSomething() { }
}
