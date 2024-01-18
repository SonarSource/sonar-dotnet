using System;
using System.Runtime.Serialization;

namespace MyLibrary
{
    public class MyException : Exception // Noncompliant {{Implement the missing constructors for this exception.}}
//               ^^^^^^^^^^^
    {
        public MyException()
        {
        }
    }

    public class My_01_Exception : Exception // Noncompliant
    {
        My_01_Exception() {}

        My_01_Exception(string message) { }

        My_01_Exception(string message, Exception innerException) {}

        My_01_Exception(SerializationInfo info, StreamingContext context) {}
    }

    public class My_02_Exception : Exception // Compliant
    {
        public My_02_Exception() { }

        public My_02_Exception(string message) { }

        public My_02_Exception(string message, Exception innerException) { }

        public My_02_Exception(SerializationInfo info, StreamingContext context) { } // optional serialization constructor (should be protected)
    }

    public class My_03_Exception : Exception // Compliant
    {
        public My_03_Exception() { }

        public My_03_Exception(string message) { }

        public My_03_Exception(string message, Exception innerException) { }

        private My_03_Exception(SerializationInfo info, StreamingContext context) { }  // optional serialization constructor (should be protected)
    }

    public sealed class My_04_Exception : Exception // Compliant
    {
        public My_04_Exception() { }

        public My_04_Exception(string message) { }

        public My_04_Exception(string message, Exception innerException) { }

        protected My_04_Exception(SerializationInfo info, StreamingContext context) { }  // optional serialization constructor (should be private)
    }

    public sealed class My_05_Exception : Exception
    {
        public My_05_Exception() { }

        public My_05_Exception(string message) { }

        public My_05_Exception(string message, Exception innerException) { }

        private My_05_Exception(SerializationInfo info, StreamingContext context) { }
    }

    public sealed class My_06_Exception : Exception // Compliant
    {
        public My_06_Exception() { }

        public My_06_Exception(string message) { }

        public My_06_Exception(string message, Exception innerException) { }

        public My_06_Exception(SerializationInfo info, StreamingContext context) { } // optional serialization constructor (should be protected)
    }
}
