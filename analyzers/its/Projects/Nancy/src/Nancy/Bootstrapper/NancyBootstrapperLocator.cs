﻿namespace Nancy.Bootstrapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Nancy.Extensions;

    /// <summary>
    /// Class for locating an INancyBootstrapper implementation.
    ///
    /// Will search the app domain for a non-abstract one, and if it can't find one
    /// it will use the default nancy one that uses TinyIoC.
    /// </summary>
    public static class NancyBootstrapperLocator
    {
        private static INancyBootstrapper instance;
        private static readonly AssemblyName NancyAssemblyName = typeof(INancyEngine).GetTypeInfo().Assembly.GetName();

        /// <summary>
        /// Gets the located bootstrapper
        /// </summary>
        public static INancyBootstrapper Bootstrapper
        {
            get { return instance ?? (instance = LocateBootstrapper()); }
            set { instance = value; }
        }

        private static INancyBootstrapper LocateBootstrapper()
        {
            var bootstrapperType = GetBootstrapperType();

            try
            {
                return Activator.CreateInstance(bootstrapperType) as INancyBootstrapper;
            }
            catch (Exception ex)
            {
                var errorMessage = string.Format("Could not initialize bootstrapper of type '{0}'.", bootstrapperType.FullName);
                throw new BootstrapperException(errorMessage, ex);
            }
        }

        private static ITypeCatalog GetDefaultTypeCatalog()
        {
            var assemblyCatalog = GetAssemblyCatalog();

            return new DefaultTypeCatalog(assemblyCatalog);
        }

        private static IReadOnlyCollection<Type> GetAvailableBootstrapperTypes(ITypeCatalog types)
        {
            return types.GetTypesAssignableTo<INancyBootstrapper>(TypeResolveStrategies.ExcludeNancy);
        }

        private static IAssemblyCatalog GetAssemblyCatalog()
        {
#if CORE
            return new DependencyContextAssemblyCatalog();
#else
            return new AppDomainAssemblyCatalog();
#endif
        }

#if !CORE
        private static bool IsNancyReferencing(Assembly assembly)
        {
            if (AssemblyName.ReferenceMatchesDefinition(assembly.GetName(), NancyAssemblyName))
            {
                return true;
            }

            foreach (var referencedAssemblyName in assembly.GetReferencedAssemblies())
            {
                if (AssemblyName.ReferenceMatchesDefinition(referencedAssemblyName, NancyAssemblyName))
                {
                    return true;
                }
            }

            return false;
        }
#endif

        internal static Type GetBootstrapperType()
        {
            return GetBootstrapperType(GetDefaultTypeCatalog());
        }

        internal static Type GetBootstrapperType(ITypeCatalog typeCatalog)
        {
            var customBootstrappers = GetAvailableBootstrapperTypes(typeCatalog);

            if (!customBootstrappers.Any())
            {
                return typeof(DefaultNancyBootstrapper);
            }

            if (customBootstrappers.Count == 1)
            {
                return customBootstrappers.Single();
            }

            Type bootstrapper;
            if (TryFindMostDerivedType(customBootstrappers, out bootstrapper))
            {
                return bootstrapper;
            }

            var errorMessage = GetMultipleBootstrappersMessage(customBootstrappers);

            throw new BootstrapperException(errorMessage);
        }

        internal static bool TryFindMostDerivedType(IReadOnlyCollection<Type> customBootstrappers, out Type bootstrapper)
        {
            var set = new HashSet<Type>();
            bootstrapper = null;

            if (customBootstrappers.All(b => set.Add(b.GetTypeInfo().BaseType)))
            {
                var except = customBootstrappers.Except(set).ToList();
                bootstrapper = except.Count == 1 ? except[0] : null;
            }

            return bootstrapper != null;
        }

        private static string GetMultipleBootstrappersMessage(IEnumerable<Type> customBootstrappers)
        {
            var bootstrapperNames = customBootstrappers.Select(x => string.Concat(" - ", x.FullName));

            var bootstrapperList = string.Join(Environment.NewLine, bootstrapperNames);

            return string.Join(Environment.NewLine, new[]
            {
                "Located multiple bootstrappers:",
                bootstrapperList,
                string.Empty,
                "Either remove unused bootstrapper types or specify which type to use."
            });
        }
    }
}
