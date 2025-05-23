namespace Nancy.Extensions
{
    using System;
    using Nancy.ErrorHandling;

    /// <summary>
    /// Containing extensions for <see cref="INancyModule"/> implementations.
    /// </summary>
    public static class ModuleExtensions
    {
        /// <summary>
        /// Extracts the friendly name of a Nancy module given its type.
        /// </summary>
        /// <param name="module">The module instance</param>
        /// <returns>A string containing the name of the parameter.</returns>
        public static string GetModuleName(this INancyModule module)
        {
            var typeName = module.GetType().Name;

            var offset = typeName.LastIndexOf("Module", StringComparison.Ordinal);

            if (offset <= 0)
            {
                return typeName;
            }

            return typeName.Substring(0, offset);
        }

        /// <summary>
        /// Returns a boolean indicating whether the route is executing, or whether the module is
        /// being constructed.
        /// </summary>
        /// <param name="module">The module instance</param>
        /// <returns>True if the route is being executed, false if the module is being constructed</returns>
        public static bool RouteExecuting(this INancyModule module)
        {
            return module.Context != null;
        }

        /// <summary>
        /// Adds the before delegate to the Before pipeline if the module is not currently executing,
        /// or executes the delegate directly and returns any response returned if it is.
        /// Uses <see cref="RouteExecutionEarlyExitException"/>
        /// </summary>
        /// <param name="module">Current module</param>
        /// <param name="beforeDelegate">Delegate to add or execute</param>
        /// <param name="earlyExitReason">Optional reason for the early exit (if necessary)</param>
        public static void AddBeforeHookOrExecute(this INancyModule module, Func<NancyContext, Response> beforeDelegate, string earlyExitReason = null)
        {
            if (module.RouteExecuting())
            {
                var result = beforeDelegate.Invoke(module.Context);

                if (result != null)
                {
                    throw new RouteExecutionEarlyExitException(result, earlyExitReason);
                }
            }
            else
            {
                module.Before.AddItemToEndOfPipeline(beforeDelegate);
            }
        }
    }
}
