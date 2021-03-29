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
            throw new NotImplementedException();
        }
    }
}
