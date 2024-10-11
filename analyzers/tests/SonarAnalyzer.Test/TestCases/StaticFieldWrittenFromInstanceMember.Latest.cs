using System;

Record.count++; // Compliant - static field set from static method
Structure.count++;                                        // Compliant - static field set from static method
RecordStructure.count++;                                  // Compliant - static field set from static method

record Record
{
    internal static int count = 0;
//                      ^^^^^^^^^ Secondary
//                      ^^^^^^^^^ Secondary@-1

    public void Method() => count++; // Noncompliant

    public static void DoSomethingStatic() => count++; // Compliant

    public static Action<int> StaticFoo() => static x =>
    {
        count += x; // compliant because it can only be used in a static context
    };

    public Action<int> Foo() => static x =>
    {
        count += x; // Noncompliant {{Make the enclosing instance method 'static' or remove this set on the 'static' field.}}
    };

    public void CallFoo() => Foo()(1); // 'count' gets incremented from an instance member
}

class Class
{
    private static string staticVar; // Secondary
    private int var;

    public int MyProperty
    {
        get { return var; }
        init
        {
            staticVar = "42"; // Noncompliant
            staticVar += "42"; // Compliant, already reported on this symbol
            var = value;
        }
    }
}

struct Structure
{
    internal static int count = 0;
    //                  ^^^^^^^^^                            Secondary    [increment]
    //                  ^^^^^^^^^                            Secondary@-1 [compoundAdd]
    //                  ^^^^^^^^^                            Secondary@-2 [deconstruct]

    public void Method() => count++;                      // Noncompliant [increment]

    public static void DoSomethingStatic() => count++;    // Compliant

    public static Action<int> StaticFoo() => static x =>
    {
        count += x;                                       // Compliant because it can only be used in a static context
    };

    public Action<int> Foo() => static x =>
    {
        count += x;                                       // Noncompliant [compoundAdd] {{Make the enclosing instance method 'static' or remove this set on the 'static' field.}}
    };

    public Action<int> Foo2() => static x =>
    {
        (count, var y) = (42, "42");
//       ^^^^^^^^^^^^^^^                                     Noncompliant [deconstruct]
    };

    public void CallFoo() => Foo()(1);                    // 'count' gets incremented from an instance member
}

record struct RecordStructure
{
    internal static int count = 0;
    //                  ^^^^^^^^^                            Secondary    [RecordIncrement]
    //                  ^^^^^^^^^                            Secondary@-1 [RecordCompoundAdd]
    //                  ^^^^^^^^^                            Secondary@-2 [RecordDeconstruct]

    public void Method() => count++;                      // Noncompliant [RecordIncrement]

    public static void DoSomethingStatic() => count++;    // Compliant

    public static Action<int> StaticFoo() => static x =>
    {
        count += x;                                       // Compliant because it can only be used in a static context
    };

    public Action<int> Foo() => static x =>
    {
        count += x;                                       // Noncompliant [RecordCompoundAdd] {{Make the enclosing instance method 'static' or remove this set on the 'static' field.}}
    };

    public Action<int> Foo2() => static x =>
    {
        (count, var y) = (42, "42");                      // Noncompliant [RecordDeconstruct]
    };

    public void CallFoo() => Foo()(1);                    // 'count' gets incremented from an instance member
}

class CSharp11
{
    private static int count = 0; // Secondary
    public int MyProperty
    {
        get { return myVar; }
        set
        {
            count >>>= 42; // Noncompliant
            myVar = value;
        }
    }

    private int myVar;
}

class CSharp13
{
    partial class PartialClass
    {
        public partial int MyProperty
        {
            get { return myVar; }
            set
            {
                count >>>= 42; // Noncompliant
                myVar = value;
            }
        }
    }

    partial class PartialClass
    {
        public partial int MyProperty { get; set; }
        private int myVar;
        private static int count = 0; // Secondary
    }
}
