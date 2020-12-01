nint i1 = 10000000;  // Noncompliant; is this 10 million or 100 million?
int i2 = 10_000_000;

nuint j1 = 0b01101001010011011110010101011110;  // Noncompliant {{Add underscores to this numeric value for readability.}}
nuint j2 = 0b01101001_01001101_11100101_01011110;
