using System;

namespace SonarAnalyzer.CBDE
{
    [Serializable]
    public sealed class CbdeException : Exception
    {
        public CbdeException() { }

        public CbdeException(string message) : base(message) { }

        public CbdeException(string message, Exception innerException) : base(message, innerException) { }
    }
}
