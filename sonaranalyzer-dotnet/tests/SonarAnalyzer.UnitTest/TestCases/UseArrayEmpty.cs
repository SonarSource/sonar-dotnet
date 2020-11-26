namespace Tests.Diagnostics
{
    public class UseArrayEmpty
    {
        public const int Zero = 0;

        public UseArrayEmpty()
        {
            var array = new int[0]; // Noncompliant {{Declare this empty array using Array.Empty<T>.}}
            //          ^^^^^^^^^^

            var dbl = new int[00]; // Noncompliant
            var other = new int[] { }; // Noncompliant
            var comment = new int[] { /* comment */ }; // Noncompliant
            var dynamic = new dynamic[] { };// Noncompliant
            var cnst = new int[Zero]; // Noncompliant

            var array_1 = new int[1]; // Compliant
            var other_1 = new[] { 17 }; // Compliant
            var multi = new int[0, 0]; // Compliant

            Arguments(new int[42] { }); // Compliant
            Arguments(new int[0]); // Noncompliant
        }

        public void Arguments(int[] arguments)
        {
        }
    }
}
