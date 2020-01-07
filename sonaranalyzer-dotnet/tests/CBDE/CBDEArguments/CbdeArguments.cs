using System;

namespace CBDEArguments
{
    public class CbdeArguments
    {
        public class CbdeArgumentException : Exception
        {
            public CbdeArgumentException(string message) : base(message)
            {
            }
        }

        public static string GetOutputPath(string[] args)
        {
            bool minusO = false;
            foreach (string s in args)
            {
                if (minusO)
                {
                    return s;
                }
                if (s == "-o")
                {
                    minusO = true;
                }
            }
            throw new CbdeArgumentException("Incorrect command line argument for CBDE executable");
        }
    }
}
