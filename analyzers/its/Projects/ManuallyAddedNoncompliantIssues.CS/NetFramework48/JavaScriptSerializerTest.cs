﻿/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

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

            new JavaScriptSerializer(new UnsafeTypeResolver()).Deserialize<string>(json); // Noncompliant
        }

        private sealed class SafeTypeResolver : JavaScriptTypeResolver
        {
            public override Type ResolveType(string id) => throw new NotImplementedException();

            public override string ResolveTypeId(Type type) => throw new NotImplementedException();
        }
    }
}
