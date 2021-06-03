using System.Threading.Tasks;
using System.Collections.Generic;

nint resultNint;
nint bitMaskNint = 0x010F;
nint nOne = 1;
nint nZero = 0;

resultNint = -1 & bitMaskNint;      // Noncompliant
resultNint = bitMaskNint | nOne;    // Compliant - FN
resultNint = bitMaskNint ^ nZero;   // Compliant - FN
resultNint = bitMaskNint ^ nZero;   // Compliant - FN
resultNint &= -nOne;                // Compliant - FN
resultNint |= nZero;                // Compliant - FN
resultNint ^= nZero;                // Compliant - FN
var result2 = resultNint ^= nZero;  // Compliant - FN

resultNint = bitMaskNint & 1;   // Compliant
resultNint = bitMaskNint | 1;   // Compliant
resultNint = bitMaskNint ^ 1;   // Compliant
resultNint &= 1;                // Compliant
resultNint |= 1;                // Compliant
resultNint ^= 1;                // Compliant

nuint bitMaskNuint = 0x010F;
nuint resultNuint;
nuint nuOne = 1;
nuint nuZero = 0;

resultNuint = bitMaskNuint & + + +nuOne;    // FN
resultNuint = bitMaskNuint & nuZero;        // Compliant
resultNuint = bitMaskNuint | 0;             // Noncompliant
resultNuint = bitMaskNuint | 0x0;           // Noncompliant
resultNuint = bitMaskNuint & returnNuint(); // Compliant
resultNuint = bitMaskNuint & 0x0F;          // Compliant

MyMethod(1 | 0x00000); // Noncompliant

static void MyMethod(nuint u) { }
static nuint returnNuint() => 1;


public class CSharp8
{
    public async Task FindConstant_AwaitForEach(IAsyncEnumerable<int> values)
    {
        var value = 0;
        var unchanged = 0;
        int result;
        await foreach (var v in values)
        {
            result = value | v;     // Compliant, value changes over iterations
            result = unchanged | v; // Noncompliant
            value = 1;
        }
        unchanged = 1;
    }
}
