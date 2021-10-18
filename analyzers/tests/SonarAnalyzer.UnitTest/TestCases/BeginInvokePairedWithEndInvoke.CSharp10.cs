using System;

namespace Tests.Diagnostics
{
    public delegate void AsyncMethodCaller(string name, int i);

    public record struct FooRecordStruct
    {
        private AsyncMethodCaller caller = null;
        public string field;

        public FooRecordStruct(string field)
        {
            this.field = field;
            caller.BeginInvoke("FooStruct", 42, null, null); // Noncompliant
        }
    }

    public record struct PositionalRecordStruct(string Property)
    {
        private AsyncMethodCaller caller = null;
        public string field = null;

        public PositionalRecordStruct(string field, string property) : this(property)
        {
            this.field = field;
            caller.BeginInvoke("FooStruct", 42, null, null); // Noncompliant
        }
    }
}
