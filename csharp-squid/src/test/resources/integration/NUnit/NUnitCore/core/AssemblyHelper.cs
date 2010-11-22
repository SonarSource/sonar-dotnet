// ****************************************************************
// Copyright 2008, Charlie Poole
// This is free software licensed under the NUnit license. You may
// obtain a copy of the license at http://nunit.org.
// ****************************************************************

using System;
using System.Reflection;

namespace NUnit.Core
{
    public class AssemblyHelper
    {
        #region GetAssemblyPath

        public static string GetAssemblyPath(Type type)
        {
            return GetAssemblyPath(type.Assembly);
        }

        public static string GetAssemblyPath(Assembly assembly)
        {
            string path = assembly.CodeBase;
            Uri uri = new Uri(path);

            // If it wasn't loaded locally, use the Location
            if (!uri.IsFile)
                return assembly.Location;

            if (uri.IsUnc)
                return path.Substring(Uri.UriSchemeFile.Length + 1);

            return uri.LocalPath;
        }

        #endregion

        #region GetDirectoryName
        public static string GetDirectoryName( Assembly assembly )
        {
            return System.IO.Path.GetDirectoryName(GetAssemblyPath(assembly));
        }
        #endregion
    }
}
