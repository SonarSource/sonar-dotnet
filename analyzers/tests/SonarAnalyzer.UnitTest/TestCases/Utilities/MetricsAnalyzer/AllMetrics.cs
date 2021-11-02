// Header comment

using System;

namespace Tests
{
    public class Program
    {
        private int noSonar; // NOSONAR
        private int field;

        public int Go(bool condition)
        {
            if (condition)
            {
                return 42;
            }

            var y = string.Empty;
            (y, var z) = ("a", 'x');

            throw new NotImplementedException();
        }
    }

    public record class RecordClass();

    public struct Struct
    {
    }

    public record struct RecordStruct();
}
