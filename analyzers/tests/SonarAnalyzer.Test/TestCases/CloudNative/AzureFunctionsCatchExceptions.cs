using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;

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

    [FunctionName("Sample")]
    public static void NoTryCatch_Body(int arg)         // Noncompliant {{Wrap Azure Function body in try/catch block.}}
    //                 ^^^^^^^^^^^^^^^
    {
        DoSomething();
    }

    [FunctionName("Sample")]
    public static void NoTryCatch_Arrow() =>            // Noncompliant
        DoSomething();

    [FunctionName("Sample")]
    public static async Task<string> NoTryCatchAsync()  // Noncompliant
    {
        return DoSomething();
    }

    [FunctionName("Sample")]
    public static void Empty()
    {
        // Compliant. Nothing to see here
    }

    [FunctionName("Sample")]
    public static void Unreachable()                    // Noncompliant
    {
        return;
        DoSomething();  // This invocation is unreachable, but still considered - this is not a SE rule.
    }

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
    public static void PropertyAccess(ICollection<int> items, NonStatic instance)   // This is considered compliant for simplicity. Properties should not throw.
    {
        if (items.Count == 0)
        {
            return;
        }
        instance.Property = 42;
    }

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
    public static void TryFinally_WithMethod()   // Noncompliant
    {
        try
        {
        }
        finally
        {
            DoSomething();
        }
    }

    [FunctionName("Sample")]
    public static void InvocationInCatch_All()
    {
        try
        {
        }
        catch
        {
            DoSomething();
        }
    }

    [FunctionName("Sample")]
    public static void InvocationInCatch_Specific() // Compliant, because the risky stuff is happening only in catch
    {
        try
        {
        }
        catch (NullReferenceException)
        {
            DoSomething();  // This is compliant, because it can be the desired logging.
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
    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
    public static void OuterCatchWhenAlwaysTrue()   // Noncompliant FP, we don't support constant condition tracking
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
    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
    public static void InvocationOutside_BeforeTry(int arg)   // Noncompliant
    {
        if (arg == 42)
        {
            DoSomething();
            try
            {
                DoSomething();
            }
            catch
            {
            }
        }
    }

    [FunctionName("Sample")]
    public static void InvocationOutside_BeforeIf(int arg)   // Noncompliant
    {
        DoSomething();
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

    [FunctionName("Sample")]
    public static void InvocationOutside_AfterTry(int arg)   // Noncompliant
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
            DoSomething();
        }
    }

    [FunctionName("Sample")]
    public static void InvocationOutside_AfterIf(int arg)   // Noncompliant
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
        DoSomething();
    }

    [FunctionName("Sample")]
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

    [FunctionName("Sample")]
    public static void NestedTry_OuterSpecificInnerAll()
    {
        try
        {
            try
            {
                DoSomething();
            }
            catch
            {
            }
        }
        catch(InvalidOperationException)
        {
        }
    }

    [FunctionName("Sample")]
    public static void NestedTry_InCatch()
    {
        try
        {
        }
        catch
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

    [FunctionName("Sample")]
    public static void NestedTry_InFinally()
    {
        try
        {
        }
        finally
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

    private static void DoSomething() { }
}

public class NonStatic
{
    public int Property { get; set; }

    [FunctionName("Sample")]
    public void InstanceMethod()         // Noncompliant
    {
        DoSomething();
    }

    [FunctionName("Sample")]
    public static void StaticMethod()   // Noncompliant
    {
        DoSomething();
    }

    private static void DoSomething() { }
}

public class ConditionalAccess
{
    public int Property { get; set; }

    [FunctionName("Sample")]
    public static void OnArgument(ConditionalAccess arg)    // Noncompliant
    {
        arg?.DoSomething();
    }

    [FunctionName("Sample")]
    public void OnInvocation_Method()       // Noncompliant
    {
        DoSomething()?.DoSomething();
    }

    [FunctionName("Sample")]
    public void OnInvocation_Property()     // Noncompliant
    {
        var value = DoSomething()?.Property;
    }

    private ConditionalAccess DoSomething() => null;
}

public class Foo
{

    // Repro for https://github.com/SonarSource/sonar-dotnet/issues/5995
    [FunctionName(nameof(CheckLicense))]
    public async Task CheckLicense()
    {
        try
        {
            //check authorization
        }
        catch (Exception e)
        {
        }
    }

    [FunctionName("Sample")]
    public async Task NameOfOutsideTry()
    {
        var x = nameof(NameOfOutsideTry);
        try
        {
        }
        catch (Exception e)
        {
        }
    }

    [FunctionName("Sample")]
    public async Task NameOfOutsideTryWithNameOfMethodInScope() // Noncompliant
    {
        var x = nameof(NameOfOutsideTryWithNameOfMethodInScope);
        try
        {
        }
        catch (Exception e)
        {
        }

        string nameof(Func<Task> x) => "";
    }
}
