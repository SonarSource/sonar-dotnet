using System;
using System.Web.Script.Serialization;

namespace Tests.Diagnostics
{
    internal class Serializer
    {
        public void JavaScriptSerializerDefaultConstructorIsSafe(string json)
        {
            new JavaScriptSerializer().Deserialize<string>(json); // Compliant - default constructor is considered safe
        }

        private static JavaScriptSerializer CtorInitializer() =>
            new JavaScriptSerializer { MaxJsonLength = int.MaxValue }; // Compliant: deserialize method is not called

        public void NullResolverIsSafe(string json)
        {
            new JavaScriptSerializer(null).Deserialize<string>(json); // Compliant - a null resolver is considered safe
        }

        public void SimpleTypeResolverIsNotSafe(string json)
        {
            new JavaScriptSerializer(new SimpleTypeResolver()).Deserialize<string>(json); // Noncompliant {{Restrict types of objects allowed to be deserialized.}}
        }

        public void CustomResolver(string json)
        {
            new JavaScriptSerializer(new UnsafeTypeResolver()).Deserialize<string>(json); // Noncompliant [unsafeResolver1]: unsafe resolver
            new JavaScriptSerializer(new SafeTypeResolver()).Deserialize<string>(json); // Compliant: safe resolver
            new JavaScriptSerializer(new UnsafeResolverWithOtherMethods()).Deserialize<string>(json); // Noncompliant [unsafeResolver2]: unsafe resolver
            new JavaScriptSerializer(new SafeTypeResolverWithOtherMethods()).Deserialize<string>(json); // Compliant: safe resolver
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
            serializer1.Deserialize<string>(json); // Noncompliant [unsafeResolver3]: unsafe resolver

            var serializer2 = new JavaScriptSerializer(safeResolver);
            serializer2.Deserialize<string>(json); // Compliant: safe resolver
        }

        public string LambdaSafe(string json) =>
            new JavaScriptSerializer(new SafeTypeResolver()).Deserialize<string>(json); // Compliant: safe resolver

        public string LambdaUnsafe(string json) =>
            new JavaScriptSerializer(new UnsafeTypeResolver()).Deserialize<string>(json); // Noncompliant [unsafeResolver4] unsafe resolver
    }

    internal class UnsafeTypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id) => Type.GetType(id);
//                           ^^^^^^^^^^^ Secondary [unsafeResolver1, unsafeResolver3, unsafeResolver4]

        public override string ResolveTypeId(Type type) => throw new NotImplementedException();
    }

    internal class SafeTypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id) => throw new NotImplementedException();

        public override string ResolveTypeId(Type type) => throw new NotImplementedException();
    }

    internal class SafeTypeResolverWithOtherMethods : JavaScriptTypeResolver
    {
        public Type BindToType(string assemblyName, string typeName) => Type.GetType(typeName);

        public Type ResolveType(string id, string wrongNumberOfParameters) => Type.GetType(id);

        public override Type ResolveType(string id) => throw new NotImplementedException();

        public override string ResolveTypeId(Type type)  => string.Empty;
    }

    internal class UnsafeResolverWithOtherMethods : JavaScriptTypeResolver
    {
        public Type BindToType(string assemblyName, string typeName) => throw new NotImplementedException();

        public Type ResolveType(string id, string wrongNumberOfParameters) => throw new NotImplementedException();

        public override Type ResolveType(string id) => Type.GetType(id);
//                           ^^^^^^^^^^^ Secondary [unsafeResolver2]

        public override string ResolveTypeId(Type type) => throw new NotImplementedException();
    }
}
