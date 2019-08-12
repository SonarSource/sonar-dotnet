using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Serialization;

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
