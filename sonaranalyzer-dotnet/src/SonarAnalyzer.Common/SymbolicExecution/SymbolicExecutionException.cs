using System;

namespace SonarAnalyzer.SymbolicExecution
{
    public sealed class SymbolicExecutionException : Exception
    {
        public SymbolicExecutionException()
        {
        }

        public SymbolicExecutionException(string message)
            : base(message)
        {
        }

        public SymbolicExecutionException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
