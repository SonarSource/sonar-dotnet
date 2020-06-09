using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    internal sealed class SafeBinderStatementWithReturnNull : SerializationBinder
    {
        public override Type BindToType(String assemblyName, System.String typeName)
        {
            if (typeName == "typeT")
            {
                return Assembly.Load(assemblyName).GetType(typeName);
            }

            return null;
        }
    }

    internal sealed class SafeBinderExpressionWithNull : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName) =>
            typeName == "typeT"
                ? Assembly.Load(assemblyName).GetType(typeName)
                : null;
    }

    internal sealed class SafeBinderWithThrowStatement : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName == "typeT")
                return Assembly.Load(assemblyName).GetType(typeName);

            throw new SerializationException("Only typeT is allowed");
        }
    }

    internal sealed class SafeBinderWithThrowExpression : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName) =>
            typeName == "typeT"
                ? Assembly.Load(assemblyName).GetType(typeName)
                : throw new SerializationException("Only typeT is allowed");
    }

    internal sealed class UnsafeBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }
    }

    internal sealed class UnsafeBinderExpressionBody : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName) =>
            Assembly.Load(assemblyName).GetType(typeName);
    }
}
