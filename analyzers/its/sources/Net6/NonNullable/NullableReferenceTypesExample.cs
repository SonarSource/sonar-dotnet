namespace NonNullable
{
    public class NullableReferenceTypesExample
    {
        private List<int>? NullableReferenceType { get; set; }

        private List<int> NonNullableReferenceType { get; set; } = new List<int>();

        public void SomeMethod(string? someString)
        {
            if (NullableReferenceType == null)
            {
                // Do Something
            }

            if (NonNullableReferenceType == null) // This check does not make sense.
            {
            }

            Console.WriteLine(someString!.Length); // This string is not null.
        }
    }
}
