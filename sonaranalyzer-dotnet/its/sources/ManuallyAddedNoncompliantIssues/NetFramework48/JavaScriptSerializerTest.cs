using System;
using System.Web.Script.Serialization;

namespace NetFramework48
{
    public class JavaScriptSerializerTest
    {
        public void SimpleTypeResolverIsNotSafe(string json)
        {
            new JavaScriptSerializer(new SimpleTypeResolver()).Deserialize<string>(json); // Noncompliant (S5773) {{Restrict types of objects allowed to be deserialized.}}

            new JavaScriptSerializer(new SafeTypeResolver()).Deserialize<string>(json);
        }

        private sealed class SafeTypeResolver : JavaScriptTypeResolver
        {
            public override Type ResolveType(string id) => throw new NotImplementedException();

            public override string ResolveTypeId(Type type) => throw new NotImplementedException();
        }
    }
}
