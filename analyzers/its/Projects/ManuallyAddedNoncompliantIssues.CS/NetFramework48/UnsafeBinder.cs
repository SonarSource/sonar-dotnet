/*
 * <Your-Product-Name>
 * Copyright (c) <Year-From>-<Year-To> <Your-Company-Name>
 *
 * Please configure this header in your SonarCloud/SonarQube quality profile.
 * You can also set it in SonarLint.xml additional file for SonarLint or standalone NuGet analyzer.
 */

using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace NetFramework48
{
    internal sealed class UnsafeBinder : SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            return Assembly.Load(assemblyName).GetType(typeName);
        }
    }
}
