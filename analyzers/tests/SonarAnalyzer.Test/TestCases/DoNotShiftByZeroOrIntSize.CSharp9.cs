using System;

// Native Integers can be either 32b or 64b depending on the underlying system.
// It's best not to raise, to avoid FPs.

nint i = 1 << 10;

i = 1 << 32;  // FN, 1 is Int32
i = (nint)1 << 32;  // Compliant

i = i << 32; // Compliant
i = i >> 32; // Compliant
i = i << 48; // Compliant
i = i << 63; // Compliant

i = i << 64; // Compliant
i = i >> 64; // Compliant
i = i << 128; // Compliant

nuint ui = 1 << 10;

ui = 1 << 32; // FN, 1 is Int32
ui = (nuint)1 << 32; // Compliant

ui = ui << 32; // Compliant
ui = ui << 48; // Compliant
ui = ui << 63; // Compliant

ui = ui << 64; // Compliant
ui = ui << 72; // Compliant
