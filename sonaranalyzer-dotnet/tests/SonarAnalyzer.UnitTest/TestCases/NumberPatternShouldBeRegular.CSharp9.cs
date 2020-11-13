nint n1 = 1000;
nint n2 = 2_435_951;
nint n3 = 0x01_00;
nint n4 = 0b1_0000_0000;
nint n5 = 0b1_00_00_00_00;

nint n6 = 1_000_00_000;  // Noncompliant {{Review this number; its irregular pattern indicates an error.}}
//        ^^^^^^^^^^^^
nint n7 = 1_234_5; // Noncompliant
nint n8 = 0b1_00_00_0000; // Noncompliant

nuint u1 = 1000;
nuint u2 = 2_435_951;
nuint u3 = 0x01_00;
nuint u4 = 0b1_0000_0000;
nuint u5 = 0b1_00_00_00_00;

nuint u6 = 1_000_00_000;  // Noncompliant {{Review this number; its irregular pattern indicates an error.}}
//         ^^^^^^^^^^^^
nuint u7 = 1_234_5; // Noncompliant
nuint u8 = 0b1_00_00_0000; // Noncompliant
