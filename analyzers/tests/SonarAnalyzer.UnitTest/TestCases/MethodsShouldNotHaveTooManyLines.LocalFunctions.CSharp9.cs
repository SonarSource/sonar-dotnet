using System;
using System.Runtime.InteropServices;

class WithExternLocalFunctions
{
    void ShortWithSingleExtern() // Compliant, we do not count extern local functions
    {
        int i = 0;
        i++;
        i++;
        i++;
        i++;

        [DllImport("libc")]
        static extern int chmod(string pathname, int mode); // Compliant, we do not count lines of an externally defined function
    }

    void ShortWithMultipleExtern() // Compliant, we do not count extern local functions
    {
        int i = 0;
        i++;
        i++;

        [DllImport("libc")] static extern int chmod1(string pathname, int mode); // Compliant
        [DllImport("libc")] static extern int chmod2(string pathname, int mode); // Compliant
        [DllImport("libc")] static extern int chmod3(string pathname, int mode); // Compliant
    }

    void Long() // Noncompliant {{This method 'Long' has 6 lines, which is greater than the 5 lines authorized. Split it into smaller methods.}}
    {
        int i = 0;
        i++;
        i++;
        i++;
        i++;
        i++;

        [DllImport("libc")]
        static extern int chmod(string pathname, int mode); // Compliant, we do not count lines of an externally defined function
    }
}
