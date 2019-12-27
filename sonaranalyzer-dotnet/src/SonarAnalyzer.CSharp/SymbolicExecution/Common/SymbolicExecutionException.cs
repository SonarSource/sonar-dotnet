using System;

namespace SonarAnalyzer.SymbolicExecution
{
    [Serializable]
    public sealed class SymbolicExecutionException : Exception
    {
        public SymbolicExecutionException() { }

        public SymbolicExecutionException(string message) : base(message) { }

        public SymbolicExecutionException(string message, Exception innerException) : base(message, innerException) { }
    }
}
