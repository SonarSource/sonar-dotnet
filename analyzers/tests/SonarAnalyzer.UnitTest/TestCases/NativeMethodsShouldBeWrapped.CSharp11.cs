﻿using System.Runtime.InteropServices;

public interface IInterface
{
    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    static extern virtual bool RemoveDirectory1(string name); // Noncompliant

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode)]
    public static extern virtual bool RemoveDirectory2(string name);  // Noncompliant
}

namespace LibraryImportAttributeTests
{
    // Copy the class to a .Net 7 project in VS and use "goto definition" on the methods to extract the code generated by the Microsoft.Interop.LibraryImportGenerator
    public static partial class ExternMethods
    {
        [LibraryImport("foo.dll")]
        public static partial void DllImportAttributeAppliedToThisFunction(); // Noncompliant

        [LibraryImport("foo.dll")]
        private static partial void CompliantDllImportAttributeAppliedToThisFunction(int i); // Compliant

        [LibraryImport("foo.dll", StringMarshalling = StringMarshalling.Utf8)]
        public static partial void DllImportAttributeAppliedToGeneratedLocalFunction(string p); // Noncompliant

        [LibraryImport("foo.dll", StringMarshalling = StringMarshalling.Utf8)]
        private static partial void CompliantDllImportAttributeAppliedToGeneratedLocalFunction(string p); // Compliant

        // Wrapper tests

        public static void CompliantDllImportAttributeAppliedToThisFunctionWrapper(int i) // Noncompliant {{Make this wrapper for native method 'CompliantDllImportAttributeAppliedToThisFunction' less trivial.}}
        {
            CompliantDllImportAttributeAppliedToThisFunction(i);
        }

        public static void CompliantDllImportAttributeAppliedToGeneratedLocalFunctionWrapper(string p) // Noncompliant
        {
            CompliantDllImportAttributeAppliedToGeneratedLocalFunction(p);
        }
    }
}
