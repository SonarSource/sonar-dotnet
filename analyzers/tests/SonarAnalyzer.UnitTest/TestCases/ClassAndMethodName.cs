using System;
using System.Runtime.InteropServices;

namespace t1
{
    class FSM // Noncompliant {{Rename class 'FSM' to match pascal case naming rules, consider using 'Fsm'.}}
//        ^^^
    {
    }

    static class IEnumerableExtensions // Compliant
    {
    }

    class foo // Noncompliant {{Rename class 'foo' to match pascal case naming rules, consider using 'Foo'.}}
    {
    }
}
namespace t2
{
    interface foo // Noncompliant {{Rename interface 'foo' to match pascal case naming rules, consider using 'IFoo'.}}
    {
    }

    interface Foo // Noncompliant  {{Rename interface 'Foo' to match pascal case naming rules, consider using 'IFoo'.}}
    {
    }

    interface IFoo
    {
    }

    interface IIFoo
    {
    }

    interface I
    {
    }

    interface II
    {
    }

    interface IIIFoo // Compliant
    {
    }

    interface IWithDefaultMembers
    {
        public void foo() { } // Noncompliant {{Rename method 'foo' to match pascal case naming rules, consider using 'Foo'.}}
    }
}
namespace t3
{
    partial class Foo
    {
    }

    class MyClass
    {
        class I
        {
        }
    }

    class IFoo2 // Noncompliant {{Rename class 'IFoo2' to match pascal case naming rules, consider using 'Foo2'.}}
    {
    }

    class Foo_2 { } // Noncompliant {{Rename class 'Foo_2' to match pascal case naming rules, consider using 'Foo2'.}}

    class Iden42TityFoo
    {
    }

    partial class
    Foo
    {
    }

    partial class
    AbClass_Bar // Noncompliant {{Rename class 'AbClass_Bar' to match pascal case naming rules, consider using 'AbClassBar'.}}
    {
    }

    struct ILMarker // Compliant
    {

    }

    [System.Runtime.InteropServices.ComImport(),
     System.Runtime.InteropServices.Guid("00000000-0000-0000-0000-000000000001")]
    internal interface SVsLog  // Compliant
    {
    }

    class IILMarker { } // Noncompliant {{Rename class 'IILMarker' to match pascal case naming rules, consider using 'IilMarker'.}}
}
namespace t4
{
    interface IILMarker { } // Compliant

    interface ITVImageScraper { }

    class A4 { }
    class AA4 { }

    class AbcDEFgh { } // Compliant
    class Ab4DEFgh { } // Compliant
    class Ab4DEFGh { } // Noncompliant {{Rename class 'Ab4DEFGh' to match pascal case naming rules, consider using 'Ab4DefGh'.}}

    class TTTestClassTTT { }// Noncompliant {{Rename class 'TTTestClassTTT' to match pascal case naming rules, consider using 'TtTestClassTtt'.}}
    class TTT44 { }// Noncompliant
    class ABCDEFGHIJK { }// Noncompliant
    class Abcd4a { }// Noncompliant

    class A_B_C { } // Noncompliant

    class AB { } // Compliant
    class AbABaa { } // Compliant
    class _AbABaa { } // Noncompliant {{Rename class '_AbABaa' to match pascal case naming rules, trim underscores from the name.}}
    class AbABaa_ { } // Noncompliant {{Rename class 'AbABaa_' to match pascal case naming rules, trim underscores from the name.}}

    class 你好 { } // Compliant
}

namespace Tests.Diagnostics
{
    public partial class ELN { } // Compliant because the other subpart is generated
}

namespace AnotherNamespace
{
    class IOStream { }
    class MyIOStream { }
    class AddUIIntegration { }
    class TokenEOF { } // Noncompliant - 3 upper case letters
    class EOFile { }  // Compliant because 2 upper case letters + 1 for the next word
}

namespace TestSuffixes
{
    // https://github.com/SonarSource/sonar-dotnet/issues/3268
    class IEnumerableExtensionsTest { } // Compliant, classes ending with "Test" or "Tests" should not be reported
    class IEnumerableExtensionsTests { } // Compliant, classes ending with "Test" or "Tests" should not be reported

    struct IStructTest { } // Noncompliant, structs are not considered as test classes
    struct IStructTests { } // Noncompliant, structs are not considered as test classes

    interface BadPrefixTest { } // Noncompliant, interfaces are not considered as test classes
    interface BadPrefixTests { } // Noncompliant, interfaces are not considered as test classes

    class _UnderscoreTest { } // Noncompliant, even when ending with Test
    class _UnderscoreTests { } // Noncompliant, even when ending with Tests

    class NormalTest { }
    class NormalTests { }

    class IMassiveProtest { } // Noncompliant, this doesn't count as Test class
    class IMassiveProtests { } // Noncompliant, this doesn't count as Test class

    class IEnumerableTEST { } // Noncompliant, this doesn't count as Test class
    class IEnumerableTESTS { } // Noncompliant, this doesn't count as Test class
    class IEnumerableTestS { } // Noncompliant, this doesn't count as Test class (upper S sufix)

    class ITest { } // Noncompliant, this doesn't count as Test class
    class ITests { } // Noncompliant, this doesn't count as Test class

    class IfTest { }
    class IfTests { }

    class XTest { }
    class XTests { }
}

namespace FPRepros
{
    // https://github.com/SonarSource/sonar-dotnet/issues/4086
    public class StructWithDllImportRepro
    {
        [DllImport("kernel32.dll", SetLastError = true, ExactSpelling = true)]
        private static extern bool ReadProcessMemory(IntPtr hProcess, IntPtr lpBaseAddress, out IMAGE_NT_HEADERS32 lpBuffer, int nSize, IntPtr lpNumberOfBytesRead);

        [StructLayout(LayoutKind.Sequential)]
        public struct IMAGE_NT_HEADERS32 // Noncompliant FP
        {
            public UInt32 Signature;
            public static int SizeOf = Marshal.SizeOf(typeof(IMAGE_NT_HEADERS32));
        }
    }

    // https://github.com/SonarSource/sonar-dotnet/issues/7238
    public class B2b // Noncompliant {{Rename class 'B2b' to match pascal case naming rules, consider using 'B2B'.}}
    { }

    public class B2bSomethingCalculator // Noncompliant FP
    { }

    public class L10nService // Noncompliant FP
    { }
}
