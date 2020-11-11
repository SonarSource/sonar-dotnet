using System;

// Native Integers can be either 32b or 64b depending on the underlying system.
// We should only raise when the shift is done with values >= 64.

nint i = 1 << 10;

i = 1 << 32;  // FN, 1 is Int32
i = (nint)1 << 32;  // Compliant

i = i << 32; // Compliant
i = i >> 32; // Compliant
i = i << 48; // Compliant
i = i << 63; // Compliant

i = i << 64; // FN
i = i >> 64; // FN
i = i << 128; // FN

nuint ui = 1 << 10;

ui = 1 << 32; // FN, 1 is Int32
ui = (nuint)1 << 32; // Compliant

ui = ui << 32; // Compliant
ui = ui << 48; // Compliant
ui = ui << 63; // Compliant

ui = ui << 64; // Compliant
ui = ui << 72; // Compliant
