class TestCases
{
        // Method is indentation is bigger than the default (8 spaces instead of 4).
        // After codefix, method itself and line "int c" is on expected indent, while "int d" gets missaligned.
        public void WrongIndendation()
        {
            int c, d; // Noncompliant
        }
}
