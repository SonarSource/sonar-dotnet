﻿using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Threading.Tasks;

public class ExpressionBodyProperties
{
    private int field;

    private int Property01
    {
        get => field;
        set => field = value; // Noncompliant
    }

    private int Property02
    {
        get => field; // Noncompliant
        set => field = value;
    }

    public void Method()
    {
        int x;

        x = Property01;
        Property02 = x;
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/2478
public class ReproIssue2478
{
    public void SomeMethod()
    {
        var (a, (barA, barB)) = new PublicDeconstructWithInnerType();

        var (_, _, c) = new PublicDeconstruct();

        var qix = new MultipleDeconstructors();
        object b;
        (a, b, c) = qix;

        (a, b) = ReturnFromMethod();

        (a, b) = new ProtectedInternalDeconstruct();

        (a, b, c) = new Ambiguous(); // Error [CS0121]
        (a, b) = new NotUsedDifferentArgumentCount();   // Error [CS1501, CS8129]
        (a, b) = new NotUsedNotVisible();               // Error [CS1061, CS8129]
    }

    internal void InternalMethod(InternalDeconstruct bar)
    {
        var (a, b) = bar;
    }

    private sealed class PublicDeconstructWithInnerType
    {
        public void Deconstruct(out object a, out InternalDeconstruct b) { a = b = null; }

        private void Deconstruct(out object a, out object b) { a = b = null; } // Compliant, Deconstruct methods are ignored
    }

    internal sealed class InternalDeconstruct
    {
        internal void Deconstruct(out object a, out object b) { a = b = null; }

        private void Deconstruct(out object a, out string b, out string c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
    }

    private class PublicDeconstruct
    {
        public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; }

        protected void Deconstruct(out string a, out string b, out string c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
        private void Deconstruct(out object a, out string b, out string c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
    }

    private sealed class MultipleDeconstructors
    {
        public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; }

        public void Deconstruct(out object a, out object b) // Compliant, Deconstruct methods are ignored
        {
            a = b = null;
        }
    }

    private class ProtectedInternalDeconstruct
    {
        protected internal void Deconstruct(out object a, out object b) { a = b = null; }

        protected internal void Deconstruct(out object a, out object b, out object c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
    }

    private class Ambiguous
    {
        public void Deconstruct(out string a, out string b, out string c) { a = b = c = null; }
        public void Deconstruct(out object a, out object b, out object c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
    }

    private class NotUsedDifferentArgumentCount
    {
        public void Deconstruct(out string a, out string b, out string c) { a = b = c = null; } // Compliant, Deconstruct methods are ignored
        public void Deconstruct(out string a, out string b, out string c, out string d) { a = b = c = d = null; } // Compliant, Deconstruct methods are ignored
    }

    private class NotUsedNotVisible
    {
        protected void Deconstruct(out object a, out object b) { a = b = null; } // Compliant, Deconstruct methods are ignored
        private void Deconstruct(out string a, out string b) { a = b = null; } // Compliant, Deconstruct methods are ignored
    }

    public class InvalidDeconstruct
    {
        private void Deconstruct(object a, out object b, out object c) { b = c = a; } // Noncompliant
        private void Deconstruct() { } // Noncompliant

        private int Deconstruct(out object a, out object b, out object c) // Noncompliant
        {
            a = b = c = null;
            return 42;
        }
    }

    private ForMethod ReturnFromMethod() => null;
    private sealed class ForMethod
    {
        public void Deconstruct(out object a, out object b) { a = b = null; }
    }
}

public class ReproIssue2333
{
    public void Method()
    {
        PrivateNestedClass x = new PrivateNestedClass();
        (x.ReadAndWrite, x.OnlyWriteNoBody, x.OnlyWrite) = ("A", "B", "C");
        var tuple = (x.ReadAndWrite, x.OnlyRead);
    }

    private class PrivateNestedClass
    {
        private string hasOnlyWrite;

        public string ReadAndWrite { get; set; }        // Setters are compliant, they are used in tuple assignment
        public string OnlyWriteNoBody { get; set; }     // Compliant, we don't raise on get without body

        public string OnlyRead
        {
            get;
            set;    // Noncompliant
        }

        public string OnlyWrite
        {
            get => hasOnlyWrite;    // Noncompliant
            set => hasOnlyWrite = value;
        }
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/2752
public class ReproIssue2752
{
    private struct PrivateStructRef
    {
        public uint part1; // Noncompliant FP. Type is communicated an external call.
    }

    private class PrivateClassRef
    {
        public uint part1; // Noncompliant FP. Type is communicated an external call.
    }

    [DllImport("user32.dll")]
    private static extern bool ExternalMethodWithStruct(ref PrivateStructRef reference);

    [DllImport("user32.dll")]
    private static extern bool ExternalMethodWithClass(ref PrivateClassRef reference);
}

public class EmptyCtor
{
    // That's invalid syntax, but it is still empty ctor and we should not raise for it, even if it is not used
    public EmptyCtor() => // Error [CS1525,CS1002]
}

public class WithEnums
{
    private enum X // Noncompliant
    {
        A
    }

    public void UseEnum()
    {
        var b = Y.B;
    }

    private enum Y
    {
        A,
        B
    }
}

// https://github.com/SonarSource/sonar-dotnet/issues/6724
public class Repro_6724
{
    public int PrivateGetter { private get; set; } // Noncompliant
    public int PrivateSetter { get; private set; } // Noncompliant

    public int ExpressionBodiedPropertyWithPrivateGetter { private get => 1; set => _ = value; } // Noncompliant
    public int ExpressionBodiedPropertyWithPrivateSetter { get => 1; private set => _ = value; } // Noncompliant
}

