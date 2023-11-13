using Microsoft;
using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
class SomeAttribute : Attribute { }

class ValidCases
{
    private void PrivateMethod1() { }                  // Compliant: not a JSInvokable
    [SomeAttribute] private void PrivateMethod2() { }  // Compliant: not a JSInvokable
}

class WithExplicitVisibilityModifiers
{
    [JSInvokable] private void PrivateMethod() { }                                   // Noncompliant {{Methods marked as 'JSInvokable' should be 'public'.}}
    //                         ^^^^^^^^^^^^^
    [JSInvokable] internal void InternalMethod() { }                                 // Noncompliant
    //                          ^^^^^^^^^^^^^^
    [JSInvokable] protected void ProtectedMethod() { }                               // Noncompliant
    //                           ^^^^^^^^^^^^^^^
    [JSInvokable] protected internal void ProtectedInternalMethod() { }              // Noncompliant
    //                                    ^^^^^^^^^^^^^^^^^^^^^^^
    [JSInvokable] public void PublicMethod() { }                                     // Compliant

    [JSInvokable] private static void PrivateStaticMethod() { }                      // Noncompliant
    //                                ^^^^^^^^^^^^^^^^^^^
    [JSInvokable] internal static void InternalStaticMethod() { }                    // Noncompliant
    //                                 ^^^^^^^^^^^^^^^^^^^^
    [JSInvokable] protected static void ProtectedStaticMethod() { }                  // Noncompliant
    //                                  ^^^^^^^^^^^^^^^^^^^^^
    [JSInvokable] protected internal static void ProtectedInternalStaticMethod() { } // Noncompliant
    //                                           ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
    [JSInvokable] public static void PublicStaticMethod() { }                        // Compliant
}

partial class WithImplicitVisibility
{
    [JSInvokable] void PrivateMethod() { } // Noncompliant: a method is implicitly private
    [JSInvokable] partial void PartialMethod2(); // Noncompliant: a partial method is implicitly private
}

unsafe class WithOtherModifiers
{
    [JSInvokable] private async Task PrivateAsyncMethod() { }   // Noncompliant
    [JSInvokable] private unsafe void PrivateUnsafeMethod() { } // Noncompliant
}

class WithMethodNamedAfterKeyword
{
    [JSInvokable] private void @for() { }  // Noncompliant
    //                         ^^^^
}

class WithMethodSignatureOnMultipleLines
{
    [JSInvokable
    ] private
        void PrivateMethod1( // Noncompliant
    //       ^^^^^^^^^^^^^^
            int i1,
            int i2)
    { }

    [
        SomeAttribute,
        JSInvokable
    ]
    private
        void PrivateMethod2( // Noncompliant
    //       ^^^^^^^^^^^^^^
            int i1,
            int i2)
    { }
}

class WithMultipleAttributes
{
    [JSInvokable, SomeAttribute] private void PrivateMethod1() { }   // Noncompliant
    [SomeAttribute, JSInvokable] private void PrivateMethod2() { }   // Noncompliant
    [JSInvokable, JSInvokable] private void PrivateMethod3() { }     // Noncompliant
    [SomeAttribute, SomeAttribute] private void PrivateMethod4() { } // Compliant: not a JSInvokable

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    private class SomeAttribute : Attribute { }
}

class WithMultipleAttributeLists
{
    [JSInvokable][SomeAttribute] private void PrivateMethod1() { }   // Noncompliant
    [SomeAttribute][JSInvokable] private void PrivateMethod2() { }   // Noncompliant
    [JSInvokable][JSInvokable] private void PrivateMethod3() { }     // Noncompliant
    [SomeAttribute][SomeAttribute] private void PrivateMethod4() { } // Compliant: not a JSInvokable

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    private class SomeAttribute : Attribute { }
}

namespace WithDifferentWaysOfReferencingTheAttribute
{
    using AliasForJSInterop = Microsoft.JSInterop;
    using AliasForJSInvokable = Microsoft.JSInterop.JSInvokableAttribute;

    class Test
    {
        [JSInvokable] private void WithoutAttributeSuffix() { }                     // Noncompliant
        [JSInvokableAttribute] private void WithAttributeSuffix() { }               // Noncompliant
        [Microsoft.JSInterop.JSInvokable] private void WithFullyQualifiedName() { } // Noncompliant
        [AliasForJSInterop.JSInvokable] private void WithNamespaceAlias() { }       // Noncompliant
        [AliasForJSInvokable] private void WithAttributeAlias() { }                 // Noncompliant
    }
}

class WithUserDefinedJSInvokable
{
    [@JSInvokable] private void PrivateMethod1() { }            // Compliant
    [JSInvokableWithSuffix] private void PrivateMethod2() { }   // Compliant

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    private class JSInvokable : Attribute { }

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    private class JSInvokableWithSuffix : Attribute { }
}
