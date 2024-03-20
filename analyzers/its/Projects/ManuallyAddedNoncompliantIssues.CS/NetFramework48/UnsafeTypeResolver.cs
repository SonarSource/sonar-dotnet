/*
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
    internal class UnsafeTypeResolver : JavaScriptTypeResolver
    {
        public override Type ResolveType(string id) => Type.GetType(id);

        public override string ResolveTypeId(Type type) => throw new NotSupportedException();
    }
}
