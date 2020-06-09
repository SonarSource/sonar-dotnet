using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace Tests.Diagnostics
{
    internal sealed class ValidBinderStatementWithReturnNull : SerializationBinder
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

    internal sealed class ValidBinderExpressionWithNull : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName) =>
            typeName == "typeT"
                ? Assembly.Load(assemblyName).GetType(typeName)
                : null;
    }

    internal sealed class ValidBinderWithThrowStatement : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (typeName == "typeT")
                return Assembly.Load(assemblyName).GetType(typeName);

            throw new SerializationException("Only typeT is allowed");
        }
    }

    internal sealed class ValidBinderWithThrowExpression : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName) =>
            typeName == "typeT"
                ? Assembly.Load(assemblyName).GetType(typeName)
                : throw new SerializationException("Only typeT is allowed");
    }

    internal sealed class InvalidBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }
    }

    internal sealed class InvalidBinderExpressionBody : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName) =>
            Assembly.Load(assemblyName).GetType(typeName);
    }
}
