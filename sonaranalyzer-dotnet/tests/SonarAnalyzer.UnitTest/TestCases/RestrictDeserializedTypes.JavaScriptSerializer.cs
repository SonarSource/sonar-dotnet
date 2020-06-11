using System;
using System.Web.Script.Serialization;

namespace Tests.Diagnostics
{
    internal class Serializer
    {
        public void SimpleTypeResolverDefaultConstructorIsNotSafe(string json)
        {
            new JavaScriptSerializer().Deserialize<string>(json); // Noncompliant: default constructor is not safe
        }

        public void NullReolverIsNotSafe(string json)
        {
            new JavaScriptSerializer(null).Deserialize<string>(json); // Noncompliant: a resolver is required for safe deserialization
        }

        public void SimpleTypeResolverIsNotSafe(string json)
        {
            new JavaScriptSerializer(new SimpleTypeResolver()).Deserialize<string>(json); // Noncompliant: SimpleTypeResolver is not safe
        }

        public void CustomResolver(string json)
        {
            new JavaScriptSerializer(new UnsafeTypeResolver()).Deserialize<string>(json); // Noncompliant: unsafe resolver
            new JavaScriptSerializer(new SafeTypeResolver()).Deserialize<string>(json); // Compliant: unsafe resolver
        }

        public void UnknownResolverType(string json, JavaScriptTypeResolver resolver)
        {
            new JavaScriptSerializer(resolver).Deserialize<string>(json); // Compliant: the resolver type is known only at runtimme
        }

        public void LocalVariable(string json)
        {
            var unsafeResolver = new UnsafeTypeResolver();
            var safeResolver = new SafeTypeResolver();

            var serializer1 = new JavaScriptSerializer(unsafeResolver);
            serializer1.Deserialize<string>(json); // Noncompliant: unsafe resolver

            var serializer2 = new JavaScriptSerializer(safeResolver);
            serializer2.Deserialize<string>(json); // Compliant: safe resolver
        }

        public string LambdaSafe(string json) =>
            new JavaScriptSerializer(new SafeTypeResolver()).Deserialize<string>(json); // Compliant: unsafe resolver

        public string LambdaUnsafe(string json) =>
            new JavaScriptSerializer(new UnsafeTypeResolver()).Deserialize<string>(json); // Noncompliant: unsafe resolver
    }

    internal class UnsafeTypeResolver : JavaScriptTypeResolver
    {
        public Type ResolveType(string id, string wrongNumberOfParameters) => throw new NotImplementedException();

        public override Type ResolveType(string id) => Type.GetType(id);

        public override string ResolveTypeId(Type type) => throw new NotImplementedException();
    }

    internal class SafeTypeResolver : JavaScriptTypeResolver
    {
        public Type ResolveType(string id, string wrongNumberOfParameters) => Type.GetType(id);

        public override Type ResolveType(string id) => throw new NotImplementedException();

        public override string ResolveTypeId(Type type) => throw new NotImplementedException();
    }
}
